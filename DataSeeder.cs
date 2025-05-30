using RAD_Demo.Models;
using System.Text.Json;

namespace RAD_Demo.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Drivers.Any())
        {
            var driversJson = File.ReadAllText("drivers.json");
            var drivers = JsonSerializer.Deserialize<List<Driver>>(driversJson);
            if (drivers != null) context.Drivers.AddRange(drivers);
        }

        if (!context.Rides.Any())
        {
            var ridesJson = File.ReadAllText("rides.json");
            var rides = JsonSerializer.Deserialize<List<Ride>>(ridesJson);
            if (rides != null) context.Rides.AddRange(rides);
        }

        context.SaveChanges();
    }
}