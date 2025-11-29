using Dapper;
using eCommerce.ProductsService.Application.Interfaces.Persistence;
using eCommerce.ProductsService.Domain.Entities;
using eCommerce.ProductsService.Infrastructure.Persistence.Context;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.ProductsService.Infrastructure.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    private readonly ApplicationDbContext _context = context;

    public IQueryable<Product> GetAllProductsQueryable()
    {
        var query = _context.Products.AsQueryable();
        return query;
    }

    public async Task<Product> GetProductByIdAsync(Guid productID, CancellationToken cancellationToken)
    {
        // Use EF Core instead of raw Dapper SQL to avoid conversion issues with GUID mapping
        var product = await _context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProductID == productID, cancellationToken);

        return product!;
    }
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void UpdateProduct(Product product)
    {
        _context.Products.Update(product);
    }

    public async Task DeleteProductAsync(Guid productID, CancellationToken cancellationToken)
    {
        Product product = await 
            GetProductByIdAsync(productID, cancellationToken);
        _context.Products.Remove(product);
    }
}
