using RAD_Demo.Models;
using System.Text.Json;

namespace RAD_Demo.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Customers.Any())
        {
            var customersJson = File.ReadAllText("customers.json");
            var customers = JsonSerializer.Deserialize<List<Customer>>(customersJson);
            if (customers != null)
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }
        }

        if (!context.Drivers.Any())
        {
            var driversJson = File.ReadAllText("drivers.json");
            var drivers = JsonSerializer.Deserialize<List<Driver>>(driversJson);
            if (drivers != null)
            {
                context.Drivers.AddRange(drivers);
                context.SaveChanges();
            }
        }

        if (!context.Rides.Any())
        {
            var ridesJson = File.ReadAllText("rides.json");
            var rides = JsonSerializer.Deserialize<List<Ride>>(ridesJson);
            if (rides != null)
            {
                foreach (var ride in rides)
                {
                    if (!context.Customers.Any(c => c.Id == ride.CustomerId))
                    {
                        context.Customers.Add(new Customer(ride.CustomerId, "Unknown Customer"));
                        context.SaveChanges();
                    }
                    if (!context.Drivers.Any(d => d.Id == ride.DriverId))
                    {
                        context.Drivers.Add(new Driver(ride.DriverId, "Unknown Driver", "Unknown Location"));
                        context.SaveChanges();
                    }
                    context.Rides.Add(ride);
                }
                context.SaveChanges();
            }
        }
    }
}