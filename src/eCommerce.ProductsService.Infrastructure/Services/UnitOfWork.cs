using eCommerce.ProductsService.Application.Interfaces.Persistence;
using eCommerce.ProductsService.Application.Interfaces.Services;
using eCommerce.ProductsService.Infrastructure.Persistence.Context;

namespace eCommerce.ProductsService.Infrastructure.Services;

public class UnitOfWork(ApplicationDbContext context,
    IProductRepository productRepository) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    public IProductRepository ProductRepository { get; } = productRepository;

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
