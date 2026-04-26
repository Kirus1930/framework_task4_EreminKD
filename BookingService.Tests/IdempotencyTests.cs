using System.Threading.Tasks;
using BookingService.Domain;
using BookingService.Infrastructure;
using BookingService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class IdempotencyTests
{
    [Fact]
    public async Task Duplicate_Event_Should_Not_Change_State()
    {
        var repo = new InMemoryProcessRepository();
        var logger = new Mock<ILogger<BookingService>>();
        var service = new BookingService(repo, logger.Object);

        await service.HandleEvent("1", "dup", "c1", "create");
        await service.HandleEvent("1", "dup", "c1", "create");

        var process = repo.GetOrCreate("1");

        Assert.Equal(BookingState.Created, process.State);
        Assert.Single(process.ProcessedEvents);
    }
}