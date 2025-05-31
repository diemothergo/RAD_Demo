namespace RAD_Demo.Models;

public enum RideStatus
{
    Booked,
    InProgress,
    Completed,
    Cancelled
}

public class Ride(string id, string customerId, string driverId, string pickupLocation, string dropoffLocation)
{
    public string Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
    public string CustomerId { get; init; } = customerId ?? throw new ArgumentNullException(nameof(customerId));
    public string DriverId { get; init; } = driverId ?? throw new ArgumentNullException(nameof(driverId));
    public string PickupLocation { get; init; } = pickupLocation ?? throw new ArgumentNullException(nameof(pickupLocation));
    public string DropoffLocation { get; init; } = dropoffLocation ?? throw new ArgumentNullException(nameof(dropoffLocation));
    public Customer? Customer { get; set; }
    public Driver? Driver { get; set; }
    public RideStatus Status { get; set; }
    public int ETA { get; set; }
}