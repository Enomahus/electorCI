using System.ComponentModel.DataAnnotations;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class DocumentDao : EntityBaseDao<Guid>
    {
        [Required]
        [MaxLength(100)]
        public required string FileName { get; set; }

        [Required]
        [MaxLength(80)]
        public required string ContentType { get; set; }

        [Required]
        public required long FileSize { get; set; }
    }
}
