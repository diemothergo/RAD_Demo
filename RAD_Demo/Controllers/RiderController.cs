using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAD_Demo.Services;
using RAD_Demo.Models;

namespace RAD_Demo.Controllers;

public class RideController : Controller
{
    private readonly BookingManager _bookingManager;
    private readonly ILogger<RideController> _logger;

    public RideController(BookingManager bookingManager, ILogger<RideController> logger)
    {
        _bookingManager = bookingManager;
        _logger = logger;
    }

    [Authorize]
    public IActionResult Index()
    {
        _logger.LogInformation("Accessing Ride/Index. IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthenticated user accessing Ride/Index, redirecting to Account/Login");
            return RedirectToAction("Login", "Account");
        }
        ViewBag.Step = "Index";
        return View();
    }

    [Authorize]
    public IActionResult Book()
    {
        _logger.LogInformation("Accessing Ride/Book. IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthenticated user accessing Ride/Book, redirecting to Account/Login");
            return RedirectToAction("Login", "Account");
        }
        ViewBag.Step = "Book";
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Book(string pickupLocation, string dropoffLocation)
    {
        _logger.LogInformation("Attempting to book ride. Pickup: {Pickup}, Dropoff: {Dropoff}, IsAuthenticated: {IsAuthenticated}",
            pickupLocation, dropoffLocation, User.Identity?.IsAuthenticated);
        ViewBag.Step = "Book";

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthenticated user attempting to book ride, redirecting to Account/Login");
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(pickupLocation) || string.IsNullOrWhiteSpace(dropoffLocation))
        {
            _logger.LogWarning("Invalid booking: Pickup or Dropoff is empty. Pickup: {Pickup}, Dropoff: {Dropoff}",
                pickupLocation, dropoffLocation);
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ điểm đón và điểm đến.");
            return View();
        }

        if (pickupLocation.Trim().Equals(dropoffLocation.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid booking: Pickup and Dropoff are the same");
            ModelState.AddModelError("", "Điểm đón và điểm đến không thể giống nhau.");
            return View();
        }

        try
        {
            if (string.IsNullOrEmpty(User.Identity?.Name))
            {
                _logger.LogError("User.Identity.Name is null or empty");
                throw new InvalidOperationException("User is not authenticated");
            }

            // Lấy hoặc tạo Customer dựa trên User.Identity.Name
            var customer = new Customer(Guid.NewGuid().ToString(), User.Identity.Name);
            _logger.LogInformation("Booking ride for customer: {CustomerId}, {CustomerName}", customer.Id, customer.Name);

            var ride = _bookingManager.BookRide(customer, pickupLocation.Trim(), dropoffLocation.Trim());
            _logger.LogInformation("Ride {RideId} booked successfully for user {User}", ride.Id, User.Identity.Name);
            TempData["SuccessMessage"] = $"Đặt xe thành công! Mã chuyến xe: {ride.Id}";
            return RedirectToAction("History");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Không có tài xế nào khả dụng"))
        {
            _logger.LogWarning(ex, "No drivers available for booking");
            ModelState.AddModelError("", "Hiện tại không có tài xế nào khả dụng. Vui lòng thử lại sau.");
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking ride for user {User}. Exception: {Message}",
                User.Identity?.Name ?? "Unknown", ex.Message);
            ModelState.AddModelError("", $"Có lỗi xảy ra khi đặt xe: {ex.Message}. Vui lòng thử lại.");
            return View();
        }

    }
    [Authorize]
    public IActionResult History()
    {
        var rides = _bookingManager.GetAllRides(); // optionally filter by customer
        return View(rides);
    }
    [Authorize]
    [HttpPost]
    public IActionResult Complete(string rideId)
    {
        try
        {
            _bookingManager.CompleteRide(rideId);
            TempData["SuccessMessage"] = "Chuyến xe đã hoàn tất và thanh toán thành công.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi hoàn tất chuyến xe");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi hoàn tất chuyến xe.";
        }

        return RedirectToAction("History");
    }

}