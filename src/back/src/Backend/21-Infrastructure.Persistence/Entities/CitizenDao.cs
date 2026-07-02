using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class CitizenDao : EntityBaseDao<Guid>, ITimestampedEntity
    {
        public Gender Gender { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        public string Nationality { get; set; }
        public string Email { get; set; }

        [Required]
        public DateTimeOffset BirthDate { get; set; }

        [Required, MaxLength(50)]
        public string BirthPlace { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public string MarriedName { get; set; }
        public string Profession { get; set; }
        public string PhysicalAddress { get; set; }
        public string PostalAddress { get; set; }
        public byte[] PassportPhoto { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? FatherId { get; set; }

        [ForeignKey(nameof(FatherId))]
        public virtual CitizenDao Father { get; set; }
        public Guid? MotherId { get; set; }

        [ForeignKey(nameof(MotherId))]
        public virtual CitizenDao Mother { get; set; }

        // Relation un-à-un vers l'électeur
        public virtual ElectorDao ElectorProfil { get; set; }
        public virtual ICollection<RegistrationRequestDao> RegistrationRequests { get; set; }
    }
}
