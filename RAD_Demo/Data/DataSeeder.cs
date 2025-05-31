using RAD_Demo.Models;

namespace RAD_Demo.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Drivers.Any())
        {
            var drivers = new List<Driver>
            {
                new Driver("D1", "Driver 1", true) { CurrentLocation = "Hanoi" },
                new Driver("D2", "Driver 2", true) { CurrentLocation = "HCMC" }
            };
            context.Drivers.AddRange(drivers);
            context.SaveChanges();
        }

        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                new Customer("C1", "Customer 1"),
                new Customer("C2", "Customer 2")
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}