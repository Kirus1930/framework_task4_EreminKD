using System;
using System.Collections.Generic;

namespace BookingService.Domain;

public class BookingProcess
{
    public string ProcessId { get; set; } = default!;
    public BookingState State { get; set; } = BookingState.None;

    public HashSet<string> ProcessedEvents { get; set; } = new();

    public DateTime LastUpdated { get; set; }
}