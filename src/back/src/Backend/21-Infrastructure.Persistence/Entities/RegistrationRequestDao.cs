using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class RegistrationRequestDao : EntityBaseDao<Guid>
    {
        [MaxLength(25)]
        [Required]
        public string Reference { get; set; }
        public DateTimeOffset SubmissionDate { get; set; }
        public RegistrationRequestType RequestType { get; set; }
        public RegistrationStatus Status { get; set; }
        public string ReasonForRejection { get; set; }
        public Guid AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public UserDao Author { get; set; }
        public Guid? LastUpdaterId { get; set; }

        [ForeignKey(nameof(LastUpdaterId))]
        public UserDao LastUpdater { get; set; }
        public Guid CitizenId { get; set; }

        [ForeignKey(nameof(CitizenId))]
        public virtual CitizenDao Citizen { get; set; } = null!;
        public long DistrictId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public DistrictDao District { get; set; }
        public virtual ICollection<RegistrationRequestDocumentDao> RegistrationRequestDocuments { get; set; } =
        [];
    }
}
