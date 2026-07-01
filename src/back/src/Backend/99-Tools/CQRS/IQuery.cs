using MediatR;

namespace Tools.CQRS
{
    public interface IQuery
    {
        public class IQuery<T> : IRequest<T> { }
    }
}
