﻿
namespace SICAPI.Models.DTOs;

public class StockRealDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int StockReal { get; set; }
}
