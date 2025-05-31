using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RAD_Demo.Models;

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
            await _signInManager.SignInAsync(user, isPersistent: false);
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

        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Đăng nhập thành công!";
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
