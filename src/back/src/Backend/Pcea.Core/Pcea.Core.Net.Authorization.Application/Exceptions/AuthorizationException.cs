using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Pcea.Core.Net.Authorization.Application.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class AuthorizationException : Exception
    {
        public IDictionary<string, object?> AdditionalData { get; protected set; }

        protected AuthorizationException(
            string? message,
            IDictionary<string, object?>? additionalData = null
        )
            : base(message)
        {
            AdditionalData = additionalData ?? new Dictionary<string, object?>();
        }

        protected AuthorizationException(
            string? message,
            Exception? innerException,
            IDictionary<string, object?>? additionalData = null
        )
            : base(message, innerException)
        {
            AdditionalData = additionalData ?? new Dictionary<string, object?>();
        }

        public override string ToString()
        {
            return $"Error {Message} for {JsonSerializer.Serialize(AdditionalData)}";
        }
    }
}
