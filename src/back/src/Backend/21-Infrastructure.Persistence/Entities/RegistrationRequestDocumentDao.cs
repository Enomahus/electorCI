using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class RegistrationRequestDocumentDao : EntityBaseDao<Guid>
    {
        [Required]
        public RegistrationRequestDocumentType RegistrationRequestDocumentType { get; set; }
        public Guid RegistrationRequestId { get; set; }

        [ForeignKey(nameof(RegistrationRequestId))]
        public virtual RegistrationRequestDao RegistrationRequest { get; set; } = null!;

        public Guid DocumentId { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public DocumentDao Document { get; set; }
    }
}
