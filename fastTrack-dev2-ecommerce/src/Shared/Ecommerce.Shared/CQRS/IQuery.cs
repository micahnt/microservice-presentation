using MediatR;

namespace Ecommerce.Shared.CQRS;
public interface IQuery<out TResponse> : IRequest<TResponse>  
    where TResponse : notnull
{
}
