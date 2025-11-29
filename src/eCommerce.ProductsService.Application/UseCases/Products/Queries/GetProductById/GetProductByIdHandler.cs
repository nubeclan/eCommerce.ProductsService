using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Dtos.Products;
using eCommerce.ProductsService.Application.Interfaces.Services;
using Mapster;

namespace eCommerce.ProductsService.Application.UseCases.Products.Queries.GetProductById;

internal sealed class GetProductByIdHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<GetProductByIdResponseDto>> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        var response = new BaseResponse<GetProductByIdResponseDto>();

        try
        {
            var product = await _unitOfWork.ProductRepository
                .GetProductByIdAsync(query.ProductId, cancellationToken);

            if (product is null)
            {
                response.IsSuccess = false;
                response.Message = "Producto no encontrado.";
                return response;
            }

            response.IsSuccess = true;
            response.Data = product.Adapt<GetProductByIdResponseDto>();
            response.Message = "Consulta exitosa.";
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Ocurrió un error inesperado. {ex.Message}";
        }

        return response;
    }
}