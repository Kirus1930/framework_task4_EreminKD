namespace BookingService.Domain;

public enum BookingState
{
    None = 0,
    Created = 1,
    RoomReserved = 2,
    PaymentProcessed = 3,
    Completed = 4,
    Cancelled = 5
}