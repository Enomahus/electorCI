namespace Infrastructure.Persistence.Common
{
    public interface IEntityBaseDao<T>
    {
        T Id { get; set; }
    }
}
