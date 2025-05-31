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

            // Kiểm tra khách hàng đã tồn tại chưa
            var existingCustomer = context.Customers.FirstOrDefault(c => c.Name == customer.Name);
            if (existingCustomer != null)
            {
                customer = existingCustomer; // Dùng lại customer cũ
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

            // Đánh dấu tài xế đã bận
            driver.IsAvailable = false;

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

        // Gọi mô phỏng thanh toán
        var amount = CalculateFare(ride);
        var paymentSuccess = new PaymentSimulator().ProcessPayment(ride.Id, amount);

        if (!paymentSuccess)
        {
            throw new InvalidOperationException("Thanh toán không thành công.");
        }

        // Trả lại tài xế
        if (ride.Driver != null)
        {
            ride.Driver.IsAvailable = true;
        }

        context.SaveChanges();
    }

    public void CancelRide(string id)
    {
        var ride = GetRide(id);
        ride.Status = RideStatus.Cancelled;

        // Trả lại tài xế nếu cần
        if (ride.Driver != null)
        {
            ride.Driver.IsAvailable = true;
        }

        context.Rides.Remove(ride);
        context.SaveChanges();
    }

    // 🆕 Tính tiền đơn giản
    private double CalculateFare(Ride ride)
    {
        // Cố định 50.000đ mỗi chuyến
        return 50000;
    }
}
