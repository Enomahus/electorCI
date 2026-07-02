using System.Reflection;
using MediatR;
using Pcea.Core.Net.AuditTrail.Attributes;
using Pcea.Core.Net.AuditTrail.Interfaces;
using Pcea.Core.Net.AuditTrail.Models;

namespace Pcea.Core.Net.AuditTrail.Behaviours
{
    public class AuditBehaviour<TRequest, TResponse>(IAuditService auditService)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var res = await next();
            var attributes = request.GetType().GetCustomAttributes<AuditParametersAttribute>();

            if (!attributes.Any())
            {
                return res;
            }

            var auditRes = res as IAuditableResult;
            var user = await auditService.GetCurrentUserInfoAsync(cancellationToken: default);
            foreach (var auditParam in attributes)
            {
                var log = new AuditLog()
                {
                    Category = auditParam.Category,
                    Action = auditParam.Action,
                    AuditUser = user,
                    Subject = auditRes?.Subject,
                    AdditionalInfo = auditRes?.AdditionalInfo,
                    Timestamp = auditService.GetCurrentTimestamp(),
                };

                await auditService.SaveAuditLogAsync(log, cancellationToken: default);
            }

            return res;
        }
    }
}
