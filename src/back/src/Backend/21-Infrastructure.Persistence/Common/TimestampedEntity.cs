namespace Infrastructure.Persistence.Common;

public abstract class TimestampedEntity<T> : EntityBaseDao<T>, ITimestampedEntity
{
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
