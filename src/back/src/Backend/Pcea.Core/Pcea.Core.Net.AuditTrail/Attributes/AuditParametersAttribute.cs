using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.AuditTrail.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditParametersAttribute : Attribute
    {
        public required string Category { get; init; }
        public required string Action { get; init; }
    }
}
