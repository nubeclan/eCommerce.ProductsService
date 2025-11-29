using eCommerce.ProductsService.Application.Commons.Bases;

namespace eCommerce.ProductsService.Application.Interfaces.Services;

public interface IOrderingQuery
{
    IQueryable<T> Ordering<T>(BasePagination request, IQueryable<T> queryable) where T : class;
}
