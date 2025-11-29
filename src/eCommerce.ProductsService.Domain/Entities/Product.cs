namespace eCommerce.ProductsService.Domain.Entities;

public class Product
{
    public Guid ProductID { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
