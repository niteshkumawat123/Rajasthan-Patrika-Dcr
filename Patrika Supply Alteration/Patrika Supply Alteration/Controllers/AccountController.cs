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
    public IActionResult Login()
    {
        // If user is already logged in, redirect to dashboard
        var user = _sessionService.GetUser(HttpContext.Session);
        if (user != null)
        {
            var roles = user.RoleDetails ?? new List<RoleDetails>();
            bool canApproveHO = roles.Any(r => r.RoleId == "7");
            bool canApproveZH = roles.Any(r => r.RoleId == "4");
            bool canAdd = roles.Any(r => r.RoleId == "1" || r.RoleId == "2" || r.RoleId == "3" || r.RoleId == "4" || r.RoleId == "6");

            if (canApproveHO && !canAdd)
                return RedirectToAction("Dashboard", "HO");
            if (canApproveZH)
                return RedirectToAction("Dashboard", "ZH");
            return RedirectToAction("Dashboard", "Home");
        }

        var model = new LoginViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (string.IsNullOrEmpty(model.EmployeeId) || string.IsNullOrEmpty(model.Password))
        {
            model.ErrorMessage = "Please enter Employee ID and Password.";
            return View(model);
        }

        model.EmployeeId = model.EmployeeId.Trim().ToUpper();

        var user = await _dbService.LoginAsync(model.EmployeeId, model.Password);
        if (user == null)
        {
            model.ErrorMessage = "Invalid credentials. Please check and try again.";
            return View(model);
        }

        // If first login, redirect to change password screen
        if (user.FirstLoginFlag)
        {
            TempData["FirstLoginEmpCode"] = user.EmpCode;
            return RedirectToAction("ChangePassword");
        }

        _sessionService.SetUser(HttpContext.Session, user);

        // Determine redirect based on role(s)
        var roles = user.RoleDetails ?? new List<RoleDetails>();
        bool canAdd = roles.Any(r => r.RoleId == "1" || r.RoleId == "3" || r.RoleId == "4" || r.RoleId == "6" || r.RoleId == "2");
        bool canApproveZH = roles.Any(r => r.RoleId == "4");
        bool canApproveHO = roles.Any(r => r.RoleId == "7");

        if (canApproveHO && !canAdd)
            return RedirectToAction("Dashboard", "HO");
        if (canApproveZH)
            return RedirectToAction("Dashboard", "ZH");
        if (canAdd)
            return RedirectToAction("Dashboard", "Home");

        return RedirectToAction("Dashboard", "Home");
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        var empCode = TempData["FirstLoginEmpCode"]?.ToString();
        if (string.IsNullOrEmpty(empCode))
        {
            return RedirectToAction("Login");
        }

        var model = new ChangePasswordViewModel { EmployeeCode = empCode };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (string.IsNullOrEmpty(model.EmployeeCode))
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            model.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return View(model);
        }

        var result = await _dbService.ChangePasswordAsync(model.EmployeeCode, model.NewPassword);
        if (!result)
        {
            model.ErrorMessage = "Failed to change password. Please try again.";
            return View(model);
        }

        // After password change, login again with new password
        var user = await _dbService.LoginAsync(model.EmployeeCode, model.NewPassword);
        if (user == null)
        {
            TempData["PasswordChanged"] = "Password changed successfully. Please login with your new password.";
            return RedirectToAction("Login");
        }

        _sessionService.SetUser(HttpContext.Session, user);

        // Determine redirect based on role(s)
        var roles = user.RoleDetails ?? new List<RoleDetails>();
        bool canAdd = roles.Any(r => r.RoleId == "1" || r.RoleId == "3" || r.RoleId == "4" || r.RoleId == "6" || r.RoleId == "2");
        bool canApproveZH = roles.Any(r => r.RoleId == "4");
        bool canApproveHO = roles.Any(r => r.RoleId == "7");

        if (canApproveHO && !canAdd)
            return RedirectToAction("Dashboard", "HO");
        if (canApproveZH)
            return RedirectToAction("Dashboard", "ZH");
        if (canAdd)
            return RedirectToAction("Dashboard", "Home");

        return RedirectToAction("Dashboard", "Home");
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
        employeeId = employeeId?.Trim().ToUpper();

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
        return RedirectToAction("Login");
    }
}
