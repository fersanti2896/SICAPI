namespace SICAPI.Models.Request.Supplier;

public class CreateSupplierRequest
{
    public string BusinessName { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RFC { get; set; }
    public string? Address { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
}
