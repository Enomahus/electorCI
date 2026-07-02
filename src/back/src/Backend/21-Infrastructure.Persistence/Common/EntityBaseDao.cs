using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Common
{
    public abstract class EntityBaseDao<T> : IEntityBaseDao<T>
    {
        [Key]
        public virtual T Id { get; set; }
    }
}
