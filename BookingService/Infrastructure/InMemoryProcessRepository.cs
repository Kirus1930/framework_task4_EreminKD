using System.Collections.Concurrent;
using BookingService.Domain;

namespace BookingService.Infrastructure;

public interface IProcessRepository
{
    BookingProcess GetOrCreate(string processId);
}

public class InMemoryProcessRepository : IProcessRepository
{
    private readonly ConcurrentDictionary<string, BookingProcess> _store = new();

    public BookingProcess GetOrCreate(string processId)
    {
        return _store.GetOrAdd(processId, id => new BookingProcess
        {
            ProcessId = id,
            State = BookingState.None
        });
    }
}