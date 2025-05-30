namespace RAD_Demo.Models;

public class Ride(string id, string customerId, string driverId, string pickupLocation, string dropoffLocation)
{
    public string Id { get; set; } = id ?? throw new ArgumentNullException(nameof(id));
    public string CustomerId { get; set; } = customerId ?? throw new ArgumentNullException(nameof(customerId));
    public Customer? Customer { get; set; }
    public string DriverId { get; set; } = driverId ?? throw new ArgumentNullException(nameof(driverId));
    public Driver? Driver { get; set; }
    public string PickupLocation { get; set; } = pickupLocation ?? throw new ArgumentNullException(nameof(pickupLocation));
    public string DropoffLocation { get; set; } = dropoffLocation ?? throw new ArgumentNullException(nameof(dropoffLocation));
    public RideStatus Status { get; set; } = RideStatus.Booked;
    public int ETA { get; set; } = 15;
}

public enum RideStatus { Booked, InProgress, Completed, Cancelled }