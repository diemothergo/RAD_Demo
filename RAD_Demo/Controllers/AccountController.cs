using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RAD_Demo.Models;

namespace RAD_Demo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Trang chào mừng - điểm bắt đầu
        [AllowAnonymous]
        public IActionResult Welcome()
        {
            // Kiểm tra nếu đã đăng nhập, chuyển đến trang chính
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ride");
            }

            ViewBag.Step = "Welcome";
            return View();
        }

        // GET: Đăng ký
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã đăng nhập, chuyển về trang chính
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ride");
            }

            ViewBag.Step = "Register";
            return View();
        }

        // POST: Đăng ký
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Step = "Register";

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem email đã tồn tại chưa
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được đăng ký. Bạn có thể đăng nhập ngay.");
                        ViewBag.ShowLoginLink = true;
                        ViewBag.ExistingEmail = model.Email;
                        return View(model);
                    }

                    var user = new IdentityUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true // Để đơn giản, không yêu cầu xác nhận email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Đăng ký thành công, chuyển đến trang đăng nhập với thông báo
                        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";
                        TempData["RegisteredEmail"] = model.Email;
                        return RedirectToAction("Login");
                    }

                    // Nếu có lỗi trong quá trình tạo user
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
                    // Log error nếu cần
                }
            }

            return View(model);
        }

        // GET: Đăng nhập
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Nếu đã đăng nhập, chuyển về trang được yêu cầu hoặc trang chính
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Ride");
            }

            ViewBag.Step = "Login";
            ViewBag.ReturnUrl = returnUrl;

            // Nếu có email từ đăng ký, tự động điền vào form
            if (TempData["RegisteredEmail"] != null)
            {
                ViewBag.RegisteredEmail = TempData["RegisteredEmail"].ToString();
            }

            return View();
        }

        // POST: Đăng nhập
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewBag.Step = "Login";
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem user có tồn tại không
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError("Email", "Email này chưa được đăng ký. Vui lòng đăng ký trước.");
                        ViewBag.ShowRegisterLink = true;
                        ViewBag.UnregisteredEmail = model.Email;
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // Đăng nhập thành công
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Ride");
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại.");
                    // Log error nếu cần
                }
            }

            return View(model);
        }

        // POST: Đăng xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Welcome");
            }
            catch (Exception)
            {
                // Nếu có lỗi, vẫn redirect về Welcome
                return RedirectToAction("Welcome");
            }
        }

        // GET: Đăng xuất (cho trường hợp GET request)
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Welcome");
            }
            catch (Exception)
            {
                // Nếu có lỗi, vẫn redirect về Welcome
                return RedirectToAction("Welcome");
            }
        }

        // Trang lỗi truy cập
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}