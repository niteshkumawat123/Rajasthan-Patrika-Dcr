namespace DCRSupplyApp.Models;

public class BranchSummaryViewModel
{
    public string? ZoneCode { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public int TotalRequests { get; set; }
    public int Increases { get; set; }
    public int Decreases { get; set; }
    public int NetIncCopies { get; set; }
    public int NetDecCopies { get; set; }
    public int PushedToErp { get; set; }
}

public class ErpPushLogViewModel
{
    public decimal ReqId { get; set; }
    public string? Agcd { get; set; }
    public string? AgName { get; set; }
    public string? Publ { get; set; }
    public string? Edtn { get; set; }
    public decimal? BaseSupply { get; set; }
    public decimal? ChangedSupply { get; set; }
    public string? IncDec { get; set; }
    public DateTime? ErpPushDate { get; set; }
    public string? PushedBy { get; set; }
}

public class ReportPageViewModel
{
    public List<BranchSummaryViewModel> BranchSummary { get; set; } = new();
    public List<BranchSummaryViewModel> YesterdaySummary { get; set; } = new();
    public List<ErpPushLogViewModel> ErpPushLog { get; set; } = new();
    public DateTime SelectedDate { get; set; }
}
