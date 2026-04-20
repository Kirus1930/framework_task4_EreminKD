using System.Diagnostics.Metrics;

namespace BookingService.Observability;

public static class Metrics
{
    public static Meter Meter = new("BookingService");

    public static Counter<int> SuccessTransitions =
        Meter.CreateCounter<int>("success_transitions");

    public static Counter<int> FailedTransitions =
        Meter.CreateCounter<int>("failed_transitions");

    public static Counter<int> DuplicateEvents =
        Meter.CreateCounter<int>("duplicate_events");

    public static Counter<int> Compensations =
        Meter.CreateCounter<int>("compensations");

    public static Histogram<double> StepDuration =
        Meter.CreateHistogram<double>("step_duration_ms");
}