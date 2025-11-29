namespace eCommerce.ProductsService.Application.Abstractions.Messaging;

public interface IRequest<out TResponse> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface IQuery<out TResponse> : IRequest<TResponse> { }
