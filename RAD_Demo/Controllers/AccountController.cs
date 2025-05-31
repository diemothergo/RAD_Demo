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
    public IActionResult Register