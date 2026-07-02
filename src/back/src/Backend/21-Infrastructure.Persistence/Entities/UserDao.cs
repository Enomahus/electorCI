using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.Common.Enum;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Entities
{
    public class UserDao : IdentityUser<Guid>, IEntityBaseDao<Guid>, ITimestampedEntity
    {
        public PersonTitle Civility { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        public string EmployeeNumber { get; set; }

        [MaxLength(20)]
        public AuthProvider? AuthProvider { get; set; }
        public DateTimeOffset? DisabledDate { get; set; }

        public virtual ICollection<RefreshTokenDao> RefreshTokens { get; set; } = [];
        public virtual ICollection<UserRoleDao> UserRoles { get; set; } = [];
        public virtual ICollection<RegistrationRequestDao> CreatedRegistrationRequests { get; set; } =
        [];
        public virtual ICollection<RegistrationRequestDao> UpdatedRegistrationRequests { get; set; } =
        [];
        public virtual ICollection<UserDistrictDao> UserDistricts { get; set; } = [];

        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
