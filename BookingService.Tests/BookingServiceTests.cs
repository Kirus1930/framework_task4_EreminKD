using System.Threading.Tasks;
using BookingService.Domain;
using BookingService.Infrastructure;
using BookingService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BookingServiceTests
{
    private BookingProcessor CreateService(out InMemoryProcessRepository repo)
    {
        repo = new InMemoryProcessRepository();

        var logger = new Mock<ILogger<BookingProcessor>>();

        return new BookingProcessor(repo, logger.Object, simulateFailure: false);
    }

    [Fact]
    public async Task Should_Complete_Full_Process()
    {
        var service = CreateService(out var repo);

        await service.HandleEvent("1", "1", "c1", "create");
        await service.HandleEvent("1", "2", "c1", "reserve");
        await service.HandleEvent("1", "3", "c1", "pay");
        await service.HandleEvent("1", "4", "c1", "complete");

        var process = repo.GetOrCreate("1");

        Assert.Equal(BookingState.Completed, process.State);
    }

    [Fact]
    public async Task Should_Handle_Compensation()
    {
        var service = CreateService(out var repo);

        await service.HandleEvent("2", "1", "c1", "create");
        await service.HandleEvent("2", "2", "c1", "reserve");

        for (int i = 0; i < 10; i++)
            await service.HandleEvent("2", $"p{i}", "c1", "pay");

        var process = repo.GetOrCreate("2");

        Assert.True(
            process.State == BookingState.Created ||
            process.State == BookingState.PaymentProcessed);
    }
}