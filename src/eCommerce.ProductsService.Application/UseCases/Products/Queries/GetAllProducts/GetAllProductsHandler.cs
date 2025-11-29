using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Dtos.Products;
using eCommerce.ProductsService.Application.Interfaces.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.ProductsService.Application.UseCases.Products.Queries.GetAllProducts;

internal sealed class GetAllProductsHandler(
    IUnitOfWork unitOfWork,
    IOrderingQuery orderingQuery) : 
    IQueryHandler<GetAllProductsQuery, IEnumerable<GetAllProductsResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOrderingQuery _orderingQuery = orderingQuery;

    public async Task<BaseResponse<IEnumerable<GetAllProductsResponseDto>>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<IEnumerable<GetAllProductsResponseDto>>();

        try
        {
            var products = _unitOfWork.ProductRepository
                .GetAllProductsQueryable()
                .AsQueryable();

            if (query.NumFilter is not null && !string.IsNullOrEmpty(query.TextFilter))
            {
                switch (query.NumFilter)
                {
                    case 1:
                        products = products.Where(x => x.Name.Contains(query.TextFilter));
                        break;
                    case 2:
                        products = products.Where(x => x.Category.Contains(query.TextFilter));
                        break;
                }
            }

            query.Sort ??= "ProductID";

            var items = await _orderingQuery
                .Ordering(query, products)
                .ToListAsync(cancellationToken);

            response.IsSuccess = true;
            response.TotalRecords = await products.CountAsync(cancellationToken);
            response.Data = items.Adapt<IEnumerable<GetAllProductsResponseDto>>();
            response.Message = "Consulta exitosa.";
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Ocurrió un error inesperado. {ex.Message}";
            return response;
        }

        return response;
    }
}
