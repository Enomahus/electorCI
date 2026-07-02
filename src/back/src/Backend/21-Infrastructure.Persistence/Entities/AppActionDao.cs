using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class AppActionDao : EntityBaseDao<long>
    {
        [MaxLength(50)]
        public AppAction ActionCode { get; set; }
        public virtual ICollection<RoleDao> Roles { get; set; } = [];
        public virtual ICollection<AppPermissionDao> Permissions { get; set; } = [];
    }
}
