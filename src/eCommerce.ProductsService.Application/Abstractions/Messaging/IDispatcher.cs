using eCommerce.ProductsService.Application.Commons.Bases;

namespace eCommerce.ProductsService.Application.Abstractions.Messaging;

public interface IDispatcher
{
    Task<BaseResponse<TResponse>> Dispatch<TRequest, TResponse>(
        TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>;
}
