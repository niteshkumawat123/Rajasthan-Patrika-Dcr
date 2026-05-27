using Microsoft.AspNetCore.Mvc;
using DCRSupplyApp.Models;
using DCRSupplyApp.Services;
using DCRSupplyApp.Filters;

namespace DCRSupplyApp.Controllers;

[ServiceFilter(typeof(SessionAuthFilter))]
public class HOController : Controller
{
    private readonly OracleDbService _dbService;
    private readonly SessionService _sessionService;

    public HOController(OracleDbService dbService, SessionService sessionService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
    }

    private UserSessionModel GetUser() => _sessionService.GetUser(HttpContext.Session)!;

    [HttpGet]
    public async Task<IActionResult> Dashboard(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var stats = await _dbService.GetHOStatsAsync(user.ComCode!, date);
        var pending = await _dbService.GetHOPendingAsync(user.ComCode!);
        var model = new HODashboardViewModel
        {
            AwaitingHo = stats.awaitingHo,
            HoApproved = stats.hoApproved,
            TotalIncreased = stats.totalIncreased,
            TotalDecreased = stats.totalDecreased,
            SelectedDate = date,
            PendingRequests = pending,
            User = user
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(decimal reqId, string remarks, string agcd, string dpcd,
        string publ, string edtn, string supplyTypeCode, decimal changedSupply, DateTime? changedSupplyDate)
    {
        var user = GetUser();
        var success = await _dbService.HOApproveAsync(reqId, user.UserId!, remarks ?? "", user.ComCode!,
            user.UnitCode!, agcd, dpcd, publ, edtn, supplyTypeCode, changedSupply, changedSupplyDate);
        return Json(new { success, message = success ? "Request approved and pushed to ERP." : "Failed to approve." });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = GetUser();
        var list = await _dbService.GetHOHistoryAsync(user.ComCode!);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var branch = await _dbService.GetBranchSummaryAsync(user.ComCode!, date);
        var erp = await _dbService.GetErpPushLogAsync(user.ComCode!, date);
        var model = new ReportPageViewModel
        {
            BranchSummary = branch,
            ErpPushLog = erp,
            SelectedDate = date
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AuditTrail(decimal reqId)
    {
        var trail = await _dbService.GetAuditTrailAsync(reqId);
        return Json(trail);
    }
}
