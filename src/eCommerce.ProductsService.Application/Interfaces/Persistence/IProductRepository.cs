using eCommerce.ProductsService.Domain.Entities;

namespace eCommerce.ProductsService.Application.Interfaces.Persistence;

public interface IProductRepository
{
    IQueryable<Product> GetAllProductsQueryable();
    Task<Product> GetProductByIdAsync(
        Guid productID, CancellationToken cancellationToken);
    Task AddProductAsync(
        Product product, CancellationToken cancellationToken);
    void UpdateProduct(Product product);
    Task DeleteProductAsync(
        Guid productID, CancellationToken cancellationToken);
}
