using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Dtos.Products;

namespace eCommerce.ProductsService.Application.UseCases.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQuery : BaseFilters, 
    IQuery<IEnumerable<GetAllProductsResponseDto>>
{
}
