namespace eCommerce.ProductsService.Application.Behaviors;

public interface IValidationService
{
    Task ValidateAsync<T>(T request, CancellationToken cancellationToken);
}
