namespace RAD_Demo.Models;

public class Driver(string id, string name, string currentLocation)
{
    public string Id { get; set; } = id ?? throw new ArgumentNullException(nameof(id));
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));
    public bool IsAvailable { get; set; } = true;
    public string CurrentLocation { get; set; } = currentLocation ?? throw new ArgumentNullException(nameof(currentLocation));
    public string? CurrentRideId { get; set; }
}