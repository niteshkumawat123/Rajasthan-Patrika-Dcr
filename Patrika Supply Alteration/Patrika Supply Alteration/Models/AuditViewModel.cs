namespace DCRSupplyApp.Models;

public class AuditTrailViewModel
{
    public decimal ReqId { get; set; }
    public string? Agcd { get; set; }
    public string? AgName { get; set; }
    public string? Publ { get; set; }
    public string? Edtn { get; set; }
    public int BaseSupply { get; set; }
    public string? IncDec { get; set; }
    public int ChangedSupply { get; set; }
    public string? ReasonCode { get; set; }
    public string? Remarks { get; set; }
    public string? ZoneCode { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public DateTime? ChangedSupplyDate { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? CreationDate { get; set; }
    public string? Status { get; set; }
    public string? ApprovalLevel { get; set; }
    public string? ApprAction { get; set; }
    public string? ActionBy { get; set; }
    public DateTime? ActionDate { get; set; }
    public string? ApproverRemarks { get; set; }
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? SUBMITTEDBYNAME { get; set; }
    public string? CreationByCode { get; set; }
}
