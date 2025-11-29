using eCommerce.ProductsService.Application.Abstractions.Messaging;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommand : ICommand<bool>
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}