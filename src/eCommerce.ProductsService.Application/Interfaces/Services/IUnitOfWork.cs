using eCommerce.ProductsService.Application.Interfaces.Persistence;

namespace eCommerce.ProductsService.Application.Interfaces.Services;

public interface IUnitOfWork : IDisposable
{
    IProductRepository ProductRepository { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
