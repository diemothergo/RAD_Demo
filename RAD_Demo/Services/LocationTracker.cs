namespace RAD_Demo.Services;

public class LocationTracker
{
    public int CalculateETA()
    {
        return new Random().Next(5, 30); // Tránh Random.Shared nếu C# cũ
    }
}