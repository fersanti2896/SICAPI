using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Supplier;

public class SuppliersResponse : BaseResponse
{
    public List<SupplierDTO>? Result { get; set; }
}
