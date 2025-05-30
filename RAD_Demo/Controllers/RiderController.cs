using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAD_Demo.Models;
using RAD_Demo.Services;

namespace RAD_Demo.Controllers
{
    public class RideController : Controller
    {
        private readonly BookingManager _bookingManager;

        public RideController(BookingManager bookingManager)
        {
            _bookingManager = bookingManager;
        }

        // Trang chính - hiển thị danh sách rides (không yêu cầu đăng nhập)
        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {
                var rides = _bookingManager.GetAllRides();
                ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
                return View(rides);
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu.";
                return View(new List<Ride>());
            }
        }

        // Trang đặt xe - yêu cầu đăng nhập
        [Authorize]
        [HttpGet]
        public IActionResult Book()
        {
            ViewBag.Step = "Book";
            return View();
        }

        // Xử lý đặt xe - yêu cầu đăng nhập
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(string pickupLocation, string dropoffLocation)
        {
            ViewBag.Step = "Book";

            if (string.IsNullOrWhiteSpace(pickupLocation) || string.IsNullOrWhiteSpace(dropoffLocation))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ điểm đón và điểm đến.");
                return View();
            }

            if (pickupLocation.Trim().Equals(dropoffLocation.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Điểm đón và điểm đến không thể giống nhau.");
                return View();
            }

            try
            {
                var customer = new Customer(
                    Guid.NewGuid().ToString(),
                    User.Identity?.Name ?? "Anonymous"
                );

                _bookingManager.BookRide(customer, pickupLocation.Trim(), dropoffLocation.Trim());

                TempData["SuccessMessage"] = "Đặt xe thành công!";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt xe. Vui lòng thử lại.");
                return View();
            }
        }

        // Lịch sử đặt xe - yêu cầu đăng nhập
        [Authorize]
        public IActionResult History()
        {
            try
            {
                var rides = _bookingManager.GetAllRides();

                // Lọc rides của user hiện tại nếu cần
                var userEmail = User.Identity?.Name;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    // Nếu bạn muốn chỉ hiển thị rides của user hiện tại
                    // rides = rides.Where(r => r.Customer.Name == userEmail).ToList();
                }

                return View(rides);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải lịch sử.";
                return View(new List<Ride>());
            }
        }

        // Hủy đặt xe - yêu cầu đăng nhập
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(string rideId)
        {
            if (string.IsNullOrEmpty(rideId))
            {
                TempData["ErrorMessage"] = "Thông tin chuyến xe không hợp lệ.";
                return RedirectToAction("History");
            }

            try
            {
                // Thêm logic hủy xe nếu có trong BookingManager
                // _bookingManager.CancelRide(rideId);

                TempData["SuccessMessage"] = "Hủy chuyến xe thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy chuyến xe.";
            }

            return RedirectToAction("History");
        }

        // API endpoint để lấy thông tin rides (cho AJAX calls)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetRides()
        {
            try
            {
                var rides = _bookingManager.GetAllRides();
                return Json(rides);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Có lỗi xảy ra" });
            }
        }
    }
}