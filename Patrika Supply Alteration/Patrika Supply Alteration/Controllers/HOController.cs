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
    private readonly FirebaseNotificationService _notificationService;

    public HOController(OracleDbService dbService, SessionService sessionService, FirebaseNotificationService notificationService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
        _notificationService = notificationService;
    }

    private UserSessionModel GetUser() => _sessionService.GetUser(HttpContext.Session)!;

    private async Task<List<string?>> GetHOBranchCodesAsync()
    {
        var user = GetUser();
        var branches = await _dbService.GetHOAllowedBranchesAsync(user.EmpCode!);
        return branches.Cast<string?>().ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var branchCodes = await GetHOBranchCodesAsync();

        var stats = await _dbService.GetHOStatsAsync(user.ComCode!, date, branchCodes);
        var pending = await _dbService.GetHOPendingAsync(user.ComCode!, branchCodes);
        var model = new HODashboardViewModel
        {
            AwaitingHo = stats.awaitingHo,
            HoApproved = stats.hoApproved,
            TotalIncreased = stats.totalIncreased,
            TotalDecreased = stats.totalDecreased,
            HoRejected = stats.hoRejected,
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
        var success = await _dbService.HOApproveAsync(reqId, user.EmpCode!, remarks ?? "", user.ComCode!,
            user.UnitCode!, agcd, dpcd, publ, edtn, supplyTypeCode, changedSupply, changedSupplyDate);
        if (success)
        {
            _ = _notificationService.SendToTopicAsync("ZH", "HO Approved", $"Request #{reqId} approved by HO and pushed to ERP.");
            _ = _notificationService.SendToTopicAsync("Executive", "Request Approved", $"Request #{reqId} has been approved and pushed to ERP.");
        }
        return Json(new { success, message = success ? "Request approved and pushed to ERP." : "Failed to approve." });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(decimal reqId, string remarks)
    {
        var user = GetUser();
        var success = await _dbService.HORejectAsync(reqId, user.EmpCode!, remarks ?? "", user.ComCode!);
        if (success)
        {
            _ = _notificationService.SendToTopicAsync("ZH", "HO Rejected", $"Request #{reqId} rejected by HO.");
            _ = _notificationService.SendToTopicAsync("Executive", "Request Rejected", $"Request #{reqId} has been rejected by HO.");
        }
        return Json(new { success, message = success ? "Request rejected." : "Failed to reject." });
    }

    [HttpGet]
    public async Task<IActionResult> RejectedByMe()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryAsync(user.ComCode!, branchCodes);
        var rejected = allRequests.Where(r => r.Status == "HO_REJECTED" && r.ActionBy == user.EmpCode).ToList();
        return Json(rejected);
    }

    [HttpGet]
    public async Task<IActionResult> AwaitingHO()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var pending = await _dbService.GetHOPendingAsync(user.ComCode!, branchCodes);
        return Json(pending);
    }

    [HttpGet]
    public async Task<IActionResult> ApprovedByMe()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryApproveByMeAsync(user.ComCode!, branchCodes);
        var approved = allRequests.Where(r => r.Status == "HO_APPROVED" && r.ActionBy == user.EmpCode).ToList();
        return Json(approved);
    }

    [HttpGet]
    public async Task<IActionResult> IncreasedRequests()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryIncreaseAndDecrease(user.ComCode!, branchCodes);
        var increased = allRequests.Where(r => r.IncDec == "I" && r.Status == "HO_APPROVED").ToList();
        return Json(increased);
    }

    [HttpGet]
    public async Task<IActionResult> DecreasedRequests()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryIncreaseAndDecrease(user.ComCode!, branchCodes);
        var decreased = allRequests.Where(r => r.IncDec == "D" && r.Status == "HO_APPROVED").ToList();
        return Json(decreased);
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var list = await _dbService.GetHOHistoryAsync(user.ComCode!, branchCodes);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var previousDate = date.AddDays(-1);
        var branchCodes = await GetHOBranchCodesAsync();

        var todaySummary = await _dbService.GetBranchSummaryByBranchesAsync(user.ComCode!, date, branchCodes);
        var yesterdaySummary = await _dbService.GetBranchSummaryByBranchesAsync(user.ComCode!, previousDate, branchCodes);

        var model = new ReportPageViewModel
        {
            BranchSummary = todaySummary,
            YesterdaySummary = yesterdaySummary,
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
