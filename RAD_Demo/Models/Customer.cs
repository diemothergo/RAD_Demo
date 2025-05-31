namespace RAD_Demo.Models;

public class Customer(string id, string name)
{
    public string Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));
    public string Name { get; init; } = name ?? throw new ArgumentNullException(nameof(name));
}