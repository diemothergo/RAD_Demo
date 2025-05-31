using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RAD_Demo.Models;
using RAD_Demo.Data;

namespace RAD_Demo.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Welcome()
    {
        _logger.LogInformation("Accessing Welcome page. IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

        if (User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("User already authenticated, redirecting to Ride/Index");
            return RedirectToAction("Index", "Ride");
        }

        // 🧹 Xoá cookie đăng nhập trước đó
        Response.Cookies.Delete(".AspNetCore.Identity.Application");
        ViewBag.Step = "Welcome";
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Step = "Register";
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewBag.Step = "Register";

        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ViewBag.ShowLoginLink = true;
            ViewBag.ExistingEmail = model.Email;
            return View(model);
        }

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync(); // 🧹 Xoá phiên trước nếu có
            await _signInManager.SignInAsync(user, isPersistent: false);

            // 🆕 Tạo Customer tương ứng
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (!db.Customers.Any(c => c.Name == user.Email))
                {
                    db.Customers.Add(new Customer(Guid.NewGuid().ToString(), user.Email));
                    db.SaveChanges();
                }
            }

            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Index", "Ride");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? email = null)
    {
        ViewBag.Step = "Login";
        ViewBag.RegisteredEmail = email;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
    {
        ViewBag.Step = "Login";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email và mật khẩu là bắt buộc.");
            return View();
        }

        await _signInManager.SignOutAsync(); // 🧹 Logout tài khoản cũ trước

        // ✅ Đăng nhập chính xác bằng Identity
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError("", "Tài khoản không tồn tại.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Đăng nhập thành công!";

            // 🆕 Tạo Customer nếu chưa có
            using var scope = HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var exists = db.Customers.FirstOrDefault(c => c.Name == email);
            if (exists == null)
            {
                db.Customers.Add(new Customer(Guid.NewGuid().ToString(), email));
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Ride");
        }

        ModelState.AddModelError("", "Thông tin đăng nhập không đúng.");
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "Đăng xuất thành công!";
        return RedirectToAction("Login", "Account");
    }
}
