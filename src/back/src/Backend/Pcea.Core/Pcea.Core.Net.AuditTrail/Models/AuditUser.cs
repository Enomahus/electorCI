using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.AuditTrail.Models
{
    public class AuditUser
    {
        /// <summary>
        /// Id of the user doing the action
        /// </summary>
        public string? UserId { get; init; }

        /// <summary>
        /// Email of the user doing the action
        /// </summary>
        public string? UserName { get; init; }

        /// <summary>
        /// Id of the impersonator behind the user
        /// </summary>
        public string? ImpersonatorId { get; init; }

        /// <summary>
        /// Email of the impersonator behind the user
        /// </summary>
        public string? ImpersonatorUserName { get; init; }
    }
}
