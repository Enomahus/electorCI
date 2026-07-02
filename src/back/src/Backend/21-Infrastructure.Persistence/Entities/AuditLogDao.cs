using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class AuditLogDao : EntityBaseDao<long>
    {
        [MaxLength(100)]
        public string UserId { get; init; }

        [MaxLength(100)]
        public string UserName { get; init; }

        [Required]
        [MaxLength(100)]
        public string Category { get; init; }

        [Required]
        [MaxLength(100)]
        public string Action { get; init; }

        [MaxLength(100)]
        public string Subject { get; init; }

        [MaxLength(100)]
        public string AdditionalInfo { get; init; }

        [Required]
        public DateTimeOffset Timestamp { get; init; }
        public string Changes { get; init; }
    }
}
