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
    // ZH Approval
    public string? ZhAction { get; set; }
    public string? ZhActionBy { get; set; }
    public DateTime? ZhActionDate { get; set; }
    public string? ZhRemarks { get; set; }
    public string? ZhFromStatus { get; set; }
    public string? ZhToStatus { get; set; }
    // HO Approval
    public string? HoAction { get; set; }
    public string? HoActionBy { get; set; }
    public DateTime? HoActionDate { get; set; }
    public string? HoRemarks { get; set; }
    public string? HoFromStatus { get; set; }
    public string? HoToStatus { get; set; }
    // ERP Push
    public string? ErpPushedBy { get; set; }
    public DateTime? ErpPushedDate { get; set; }
    // Legacy fields kept for backward compat
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
