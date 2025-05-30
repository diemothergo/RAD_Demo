namespace RAD_Demo.Models;

public class Customer(string id, string name)
{
    public string Id { get; set; } = id ?? throw new ArgumentNullException(nameof(id));
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));
}