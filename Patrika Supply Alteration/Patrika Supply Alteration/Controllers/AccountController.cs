using Microsoft.AspNetCore.Mvc;
using DCRSupplyApp.Models;
using DCRSupplyApp.Services;

namespace DCRSupplyApp.Controllers;

public class AccountController : Controller
{
    private readonly OracleDbService _dbService;
    private readonly EmailService _emailService;
    private readonly SessionService _sessionService;

    public AccountController(OracleDbService dbService, EmailService emailService, SessionService sessionService)
    {
        _dbService = dbService;
        _emailService = emailService;
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> RoleSelect()
    {
        var roles = await _dbService.GetRolesAsync();
        return View(roles);
    }

    [HttpPost]
    public IActionResult RoleSelect(string code, string name)
    {
        TempData["RoleCode"] = code;
        TempData["RoleName"] = name;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginViewModel
        {
            RoleCode = TempData["RoleCode"]?.ToString(),
            RoleName = TempData["RoleName"]?.ToString()
        };
        if (string.IsNullOrEmpty(model.RoleCode))
            return RedirectToAction("RoleSelect");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _dbService.LoginAsync(model.EmployeeId, model.Password, model.RoleCode!);
        if (user == null)
        {
            model.ErrorMessage = "Invalid credentials or role. Please check and try again.";
            return View(model);
        }

        _sessionService.SetUser(HttpContext.Session, user);

        return user.HierarchyCode?.ToUpper() switch
        {
            var c when c?.Contains("ZH") == true => RedirectToAction("Dashboard", "ZH"),
            var c when c?.Contains("HO") == true => RedirectToAction("Dashboard", "HO"),
            _ => RedirectToAction("Dashboard", "Home")
        };
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string employeeId)
    {
        var (email, password) = await _dbService.GetForgotPasswordAsync(employeeId);
        if (email != null && password != null)
        {
            try
            {
                await _emailService.SendForgotPasswordEmailAsync(email, employeeId, password);
                ViewBag.Success = "Password has been sent to your registered email.";
            }
            catch
            {
                ViewBag.Error = "Failed to send email. Please try again later.";
            }
        }
        else
        {
            ViewBag.Error = "Employee ID not found.";
        }
        return View();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        _sessionService.ClearUser(HttpContext.Session);
        return RedirectToAction("RoleSelect");
    }
}
