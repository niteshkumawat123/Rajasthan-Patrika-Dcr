using Microsoft.AspNetCore.Mvc;
using DCRSupplyApp.Services;
using DCRSupplyApp.Filters;

namespace DCRSupplyApp.Controllers;

[ServiceFilter(typeof(SessionAuthFilter))]
public class ProfileController : Controller
{
    private readonly OracleDbService _dbService;
    private readonly SessionService _sessionService;

    public ProfileController(OracleDbService dbService, SessionService sessionService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = _sessionService.GetUser(HttpContext.Session)!;
        var profile = await _dbService.GetProfileAsync(user.EmpCode!);
        if(profile!=null && !string.IsNullOrEmpty(profile.EmpCode))
        {
            profile.EmpCurrentStatus = profile.EmpCurrentStatus!=null && profile.EmpCurrentStatus.Contains("03,07") ? "03" : "01";
        }

        return View(profile);
    }
}
