using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain;

namespace Infrastructure.Persistence.Entities
{
    public class AppPermissionDao : EntityBaseDao<long>, IPermission
    {
        [MaxLength(50)]
        public AppPermission PermissionCode { get; set; }
        public virtual ICollection<AppActionDao> Actions { get; set; } = [];

        [NotMapped]
        public string Code => PermissionCode.ToString();
    }
}
