using MediatR;

namespace Tools.CQRS
{
    public interface ICommand
    {
        public class ICommand<T> : IRequest<T> { }
    }
}
