namespace DCRSupplyApp.Models;

public class HODashboardViewModel
{
    public int AwaitingHo { get; set; }
    public int HoApproved { get; set; }
    public int TotalIncreased { get; set; }
    public int TotalDecreased { get; set; }
    public int HoRejected { get; set; }
    public DateTime SelectedDate { get; set; }
    public List<SupplyRequestViewModel> PendingRequests { get; set; } = new();
    public UserSessionModel? User { get; set; }
}
