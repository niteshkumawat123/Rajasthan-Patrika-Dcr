namespace DCRSupplyApp.Models;

public class SupplyViewModel
{
    public decimal? BaseSupply { get; set; }
    public decimal? SupplyMon { get; set; }
    public decimal? SupplyTue { get; set; }
    public decimal? SupplyWed { get; set; }
    public decimal? SupplyThu { get; set; }
    public decimal? SupplyFri { get; set; }
    public decimal? SupplySat { get; set; }
    public decimal? SupplySun { get; set; }
    public DateTime? SupplyEffectiveDate { get; set; }
    public string? SupplyFlag { get; set; }
    public string? Publ { get; set; }
    public string? Edtn { get; set; }
    public string? SupplyTypeCode { get; set; }
}
