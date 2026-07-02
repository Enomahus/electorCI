using System;
using System.Collections.Generic;
using System.Text;
using Pcea.Core.Net.AuditTrail.Models;

namespace Pcea.Core.Net.AuditTrail.Interfaces
{
    public interface IAuditService
    {
        Task<AuditUser> GetCurrentUserInfoAsync(CancellationToken cancellationToken = default);

        Task SaveAuditLogAsync(AuditLog log, CancellationToken cancellationToken = default);

        DateTimeOffset GetCurrentTimestamp();
    }
}
