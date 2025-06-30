using SICAPI.Models.DTOs;

namespace SICAPI.Models.Response.Products;

public class ProductsResponse : BaseResponse
{
    public List<ProductDTO>? Result { get; set; }
}
