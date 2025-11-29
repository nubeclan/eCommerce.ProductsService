using eCommerce.ProductsService.Application.Abstractions.Messaging;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommand : ICommand<bool>
{
    public Guid ProductId { get; set; }
}