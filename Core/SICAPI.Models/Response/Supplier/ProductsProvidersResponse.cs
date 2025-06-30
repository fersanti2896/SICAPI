using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Supplier;

public class ProductsProvidersResponse : BaseResponse
{
    public List<ProductBySupplierDTO>? Result { get; set; }
}
