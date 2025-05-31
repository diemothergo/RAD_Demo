using RAD_Demo.Data;
using RAD_Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace RAD_Demo.Services;

public class BookingManager
{
    private readonly AppDbContext context;

    public BookingManager(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Ride BookRide(Customer customer, string pickupLocation, string dropoffLocation)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (string.IsNullOrWhiteSpace(pickupLocation)) throw new ArgumentNullException(nameof(pickupLocation));
        if (string.IsNullOrWhiteSpace(dropoffLocation)) throw new ArgumentNullException(nameof(dropoffLocation));

        try
        {
            // Kiểm tra tài xế khả dụng
            var driver = context.Drivers.FirstOrDefault(d => d.IsAvailable);
            if (driver == null)
            {
                throw new InvalidOperationException("Không có tài xế nào khả dụng.");
            }

            // Kiểm tra khách hàng tồn tại
            var existingCustomer = context.Customers.FirstOrDefault(c => c.Name == customer.Name);
            if (existingCustomer != null)
            {
                customer = existingCustomer; // Sử dụng khách hàng hiện có
            }

            var ride = new Ride(
                id: Guid.NewGuid().ToString(),
                customerId: customer.Id,
                driverId: driver.Id,
                pickupLocation: pickupLocation,
                dropoffLocation: dropoffLocation
            )
            {
                Customer = customer,
                Driver = driver,
                Status = RideStatus.Booked,
                ETA = new LocationTracker().CalculateETA()
            };

            context.Rides.Add(ride);
            if (existingCustomer == null)
            {
                context.Customers.Add(customer);
            }

            context.SaveChanges();
            return ride;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi đặt xe: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi đặt xe: {ex.Message}", ex);
        }
    }

    public Ride GetRide(string id)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        return context.Rides
            .Include(r => r.Customer)
            .Include(r => r.Driver)
            .FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException($"Không tìm thấy chuyến xe với ID {id}");
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
        if (string.IsNullOrEmpty(driverId)) throw new ArgumentNullException(nameof(driverId));
        return context.Drivers.FirstOrDefault(d => d.Id == driverId)
            ?? throw new InvalidOperationException($"Không tìm thấy tài xế với ID {driverId}");
    }

    public void CompleteRide(string id)
    {
        var ride = GetRide(id);
        ride.Status = RideStatus.Completed;
        context.SaveChanges();
    }

    public void CancelRide(string id)
    {
        var ride = GetRide(id);
        ride.Status = RideStatus.Cancelled;
        context.Rides.Remove(ride);
        context.SaveChanges();
    }
}