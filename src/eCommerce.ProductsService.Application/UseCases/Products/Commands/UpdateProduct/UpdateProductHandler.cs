using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Behaviors;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Interfaces.Services;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.UpdateProduct;

internal sealed class UpdateProductHandler(
    IUnitOfWork unitOfWork,
    IValidationService validationService)
    : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidationService _validationService = validationService;

    public async Task<BaseResponse<bool>> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();

        try
        {
            // Validar el comando
            await _validationService.ValidateAsync(command, cancellationToken);
            
            var product = await _unitOfWork.ProductRepository
                .GetProductByIdAsync(command.ProductId, cancellationToken);

            if (product is null)
            {
                response.IsSuccess = false;
                response.Message = "Producto no encontrado.";
                return response;
            }

            product.Name = command.Name;
            product.Category = command.Category;
            product.UnitPrice = command.UnitPrice;
            product.StockQuantity = command.StockQuantity;

            _unitOfWork.ProductRepository.UpdateProduct(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.IsSuccess = true;
            response.Data = true;
            response.Message = "Producto actualizado exitosamente.";
        }
        catch (Commons.Exceptions.ValidationException ex)
        {
            response.IsSuccess = false;
            response.Message = "Errores de validación.";
            response.Errors = ex.Errors;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Error al actualizar el producto. {ex.Message}";
        }

        return response;
    }
}