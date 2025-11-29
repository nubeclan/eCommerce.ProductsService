using eCommerce.ProductsService.Application.Commons.Bases;

namespace eCommerce.ProductsService.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<BaseResponse<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
