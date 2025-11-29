using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Behaviors;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Interfaces.Services;
using eCommerce.ProductsService.Domain.Entities;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.CreateProduct;

internal sealed class CreateProductHandler(
    IUnitOfWork unitOfWork,
    IValidationService validationService)
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidationService _validationService = validationService;

    public async Task<BaseResponse<Guid>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Guid>();

        try
        {
            // Validar el comando
            await _validationService.ValidateAsync(command, cancellationToken);
            
            var product = new Product
            {
                ProductID = Guid.NewGuid(),
                Name = command.Name,
                Category = command.Category,
                UnitPrice = command.UnitPrice,
                StockQuantity = command.StockQuantity
            };

            await _unitOfWork.ProductRepository.AddProductAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.IsSuccess = true;
            response.Data = product.ProductID;
            response.Message = "Producto creado exitosamente.";
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
            response.Message = $"Error al crear el producto. {ex.Message}";
        }

        return response;
    }
}