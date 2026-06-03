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

    [HttpGet]
    public async Task<IActionResult> Dashboard(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var branchCodes = user.BranchDetails?.Select(b => b.BranchCode).Where(b => b != null).ToList();

        var stats = await _dbService.GetHOStatsAsync(user.ComCode!, date, branchCodes);
        var pending = await _dbService.GetHOPendingAsync(user.ComCode!,branchCodes);
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
        var success = await _dbService.HOApproveAsync(reqId, user.EmpCode!, remarks ?? "", user.ComCode!,
            user.UnitCode!, agcd, dpcd, publ, edtn, supplyTypeCode, changedSupply, changedSupplyDate);
        if (success)
        {
            _ = _notificationService.SendToTopicAsync("ZH", "HO Approved", $"Request #{reqId} approved by HO and pushed to ERP.");
            _ = _notificationService.SendToTopicAsync("Executive", "Request Approved", $"Request #{reqId} has been approved and pushed to ERP.");
        }
        return Json(new { success, message = success ? "Request approved and pushed to ERP." : "Failed to approve." });
    }

    [HttpGet]
    public async Task<IActionResult> FilteredRequests(string status, string? branch, string? approvedBy)
    {
        var user = GetUser();
        var allRequests = await _dbService.GetHOHistoryAsync(user.ComCode!);
        var filtered = status switch
        {
            "awaiting" => allRequests.Where(r => r.Status == "PENDING_HO").ToList(),
            "approved" => allRequests.Where(r => r.Status == "HO_APPROVED").ToList(),
            "increased" => allRequests.Where(r => r.IncDec == "I" && r.Status == "HO_APPROVED").ToList(),
            "decreased" => allRequests.Where(r => r.IncDec == "D" && r.Status == "HO_APPROVED").ToList(),
            _ => allRequests
        };

        if (!string.IsNullOrEmpty(branch))
            filtered = filtered.Where(r => r.BranchCode == branch).ToList();

        if (!string.IsNullOrEmpty(approvedBy))
            filtered = filtered.Where(r => r.ActionBy == approvedBy).ToList();

        return Json(filtered);
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
