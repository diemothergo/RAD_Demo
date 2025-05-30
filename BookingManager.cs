using RAD_Demo.Data;
using RAD_Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace RAD_Demo.Services;

public class BookingManager(AppDbContext context)
{
    public Ride BookRide(Customer customer, string pickupLocation, string dropoffLocation)
    {
        var ride = new Ride(Guid.NewGuid().ToString(), customer.Id, context.Drivers.FirstOrDefault(d => d.IsAvailable)?.Id ?? Guid.NewGuid().ToString(), pickupLocation, dropoffLocation)
        {
            Customer = customer,
            Driver = context.Drivers.FirstOrDefault(d => d.IsAvailable)
        };

        context.Rides.Add(ride);
        context.SaveChanges();
        return ride;
    }

    public Ride? GetRide(string id)
    {
        return context.Rides
            .Include(r => r.Customer)
            .Include(r => r.Driver)
            .FirstOrDefault(r => r.Id == id);
    }

    public List<Ride> GetAllRides()
    {
        return context.Rides
            .Include(r => r.Customer)
            .Include(r => r.Driver)
            .ToList();
    }

    public Driver GetDriver(string driverId)
    {
        return context.Drivers.FirstOrDefault(d => d.Id == driverId)
            ?? new Driver(Guid.NewGuid().ToString(), "Default Driver", "29H-123.45");
    }

    public void CompleteRide(string id)
    {
        var ride = GetRide(id);
        if (ride != null)
        {
            ride.Status = RideStatus.Completed;
            context.SaveChanges();
        }
    }

    public void CancelRide(string id)
    {
        var ride = GetRide(id);
        if (ride != null)
        {
            ride.Status = RideStatus.Cancelled;
            context.Rides.Remove(ride);
            context.SaveChanges();
        }
    }
}