using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.AuditTrail.Models
{
    public class AuditLog
    {
        public required AuditUser AuditUser { get; init; }

        /// <summary>
        /// Unique identifier of the subject targeted by the action
        /// </summary>
        public string? Subject { get; init; }

        /// <summary>
        /// Additional informations related to the subject
        /// </summary>
        public string? AdditionalInfo { get; init; }

        /// <summary>
        /// Category of the action triggered
        /// </summary>
        public required string Category { get; init; }

        /// <summary>
        /// Name of the action triggered
        /// </summary>
        public required string Action { get; init; }

        /// <summary>
        /// Timestamp of the action
        /// </summary>
        public required DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// Serialized view of the changes that occured (TBD)
        /// </summary>
        public string? Changes { get; init; }
    }
}
