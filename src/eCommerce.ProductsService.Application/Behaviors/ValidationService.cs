using eCommerce.ProductsService.Application.Commons.Bases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = eCommerce.ProductsService.Application.Commons.Exceptions.ValidationException;

namespace eCommerce.ProductsService.Application.Behaviors;

public class ValidationService(IServiceProvider serviceProvider) : IValidationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task ValidateAsync<T>(T request, CancellationToken cancellationToken)
    {
        var validators = _serviceProvider.GetServices<IValidator<T>>();

        if (!validators.Any()) return;

        var context = new ValidationContext<T>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(x => x.Errors.Any())
            .SelectMany(x => x.Errors)
            .Select(err => new BaseError
            {
                PropertyName = err.PropertyName,
                ErrorMessage = err.ErrorMessage
            }).ToList();

        if (failures.Any())
            throw new ValidationException(failures);
    }
}
