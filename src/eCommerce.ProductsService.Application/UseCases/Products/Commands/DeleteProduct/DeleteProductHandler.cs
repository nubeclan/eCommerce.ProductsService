using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Behaviors;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Interfaces.Services;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.DeleteProduct;

internal sealed class DeleteProductHandler(
    IUnitOfWork unitOfWork,
    IValidationService validationService)
    : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidationService _validationService = validationService;

    public async Task<BaseResponse<bool>> Handle(
        DeleteProductCommand command,
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

            await _unitOfWork.ProductRepository.DeleteProductAsync(command.ProductId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.IsSuccess = true;
            response.Data = true;
            response.Message = "Producto eliminado exitosamente.";
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
            response.Message = $"Error al eliminar el producto. {ex.Message}";
        }

        return response;
    }
}