using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BookingService.Domain;
using BookingService.Infrastructure;
using BookingService.Observability;

namespace BookingService.Services;

public class BookingProcessor
{
    private readonly IProcessRepository _repo;
    private readonly ILogger<BookingProcessor> _logger;
    private readonly bool _simulateFailure;

    public BookingProcessor(IProcessRepository repo, ILogger<BookingProcessor> logger, bool simulateFailure = true)
    {
        _repo = repo;
        _logger = logger;
        _simulateFailure = simulateFailure;
    }

    public async Task HandleEvent(
        string processId,
        string idempotencyKey,
        string correlationId,
        string action)
    {
        var process = _repo.GetOrCreate(processId);

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ProcessId"] = processId
        });

        // Идемпотентность
        if (process.ProcessedEvents.Contains(idempotencyKey))
        {
            Metrics.DuplicateEvents.Add(1);
            _logger.LogInformation("Duplicate event: {Key}", idempotencyKey);
            return;
        }

        var sw = Stopwatch.StartNew();

        try
        {
            switch (process.State)
            {
                case BookingState.None:
                    if (action == "create")
                        process.State = BookingState.Created;
                    break;

                case BookingState.Created:
                    if (action == "reserve")
                        process.State = BookingState.RoomReserved;
                    break;

                case BookingState.RoomReserved:
                    if (action == "pay")
                    {
                        if (_simulateFailure && Random.Shared.Next(0, 3) == 0)
                            throw new Exception("Payment failed");

                        process.State = BookingState.PaymentProcessed;
                    }
                    break;

                case BookingState.PaymentProcessed:
                    if (action == "complete")
                        process.State = BookingState.Completed;
                    break;
            }

            process.ProcessedEvents.Add(idempotencyKey);
            process.LastUpdated = DateTime.UtcNow;

            Metrics.SuccessTransitions.Add(1);

            _logger.LogInformation("State: {State}", process.State);
        }
        catch (Exception ex)
        {
            Metrics.FailedTransitions.Add(1);

            _logger.LogError(ex, "Failure");

            // Компенсация
            if (process.State == BookingState.RoomReserved)
            {
                process.State = BookingState.Created;

                Metrics.Compensations.Add(1);

                _logger.LogWarning("Compensation executed");
            }
        }
        finally
        {
            sw.Stop();
            Metrics.StepDuration.Record(sw.Elapsed.TotalMilliseconds);
        }

        await Task.CompletedTask;
    }
}