namespace Infrastructure.Persistence.Common;

public interface ITimestampedEntity
{
    DateTimeOffset ModifiedAt { get; set; }
    DateTimeOffset CreatedAt { get; set; }
}
