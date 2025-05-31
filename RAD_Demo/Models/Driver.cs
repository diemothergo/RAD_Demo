namespace RAD_Demo.Models;

public class Driver
{
    private readonly string _id;
    private readonly string _name;

    public Driver(string id, string name, bool isAvailable)
    {
        _id = id ?? throw new ArgumentNullException(nameof(id));
        _name = name ?? throw new ArgumentNullException(nameof(name));
        IsAvailable = isAvailable;
    }

    public string Id => _id;
    public string Name => _name;
    public bool IsAvailable { get; set; }
    public string? CurrentLocation { get; set; } // Thêm CurrentLocation
}