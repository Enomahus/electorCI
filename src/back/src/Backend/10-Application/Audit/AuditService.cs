using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Interfaces.Services;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Pcea.Core.Net.AuditTrail.Interfaces;
using Pcea.Core.Net.AuditTrail.Models;

namespace Application.Audit
{
    public class AuditService(
        TimeProvider timeProvider,
        ICurrentUserService currentUserService,
        WritableDbContext context
    ) : IAuditService
    {
        public DateTimeOffset GetCurrentTimestamp()
        {
            return timeProvider.GetUtcNow();
        }

        public Task<AuditUser> GetCurrentUserInfoAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new AuditUser()
                {
                    UserId = currentUserService.UserId?.ToString() ?? "",
                    UserName = currentUserService.UserEmail ?? "",
                    //ImpersonatorId = currentUserService.ImpersonatorId?.ToString() ?? "",
                    //ImpersonatorUserName = currentUserService.ImpersonatorEmail ?? "",
                }
            );
        }

        public async Task SaveAuditLogAsync(AuditLog log, CancellationToken cancellationToken = default)
        {
            var dao = MapLogToDao(log);
            context.AuditLogs.Add(dao);
            await context.SaveChangesAsync(cancellationToken);
        }

        private static AuditLogDao MapLogToDao(AuditLog log)
        {
            return new()
            {
                UserId = log.AuditUser.UserId,
                UserName = log.AuditUser.UserName,
                //ImpersonatorId = log.AuditUser.ImpersonatorId,
                //ImpersonatorUserName = log.AuditUser.ImpersonatorUserName,
                Category = log.Category,
                Action = log.Action,
                Subject = log.Subject,
                AdditionalInfo = log.AdditionalInfo,
                Changes = log.Changes,
                Timestamp = log.Timestamp,
            };
        }
    }
}
