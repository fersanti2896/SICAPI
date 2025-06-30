namespace SICAPI.Models.DTOs;

public class ProductDTO
{
    public int ProductId { get; set; } // Identificador del producto
    public string ProductName { get; set; } // Nombre del producto
    public string? Description { get; set; } // Descripción del producto
    public string? Barcode { get; set; } // Código de barras
    public string? Presentation { get; set; } // Caja, Frasco, Blíster
    public string? Unit { get; set; } // Unidad de medida (ej. pieza, caja, frasco)
    public decimal? Price { get; set; } // Unidad de medida (ej. pieza, caja, frasco)
}
