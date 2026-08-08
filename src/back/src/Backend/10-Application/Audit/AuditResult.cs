using Application.Models;
using Pcea.Core.Net.AuditTrail.Interfaces;

namespace Application.Audit
{
    public class AuditResult : Result, IAuditableResult
    {
        public string? Subject { get; init; }

        public string? AdditionalInfo { get; init; }

        protected AuditResult() { }

        [Obsolete("Audit result must at least have a subject")]
        public static new AuditResult Default()
        {
            throw new InvalidOperationException("Audit result must at least have a subject");
        }

        public static AuditResult From(string? subject) => new AuditResult() { Subject = subject };

        public static AuditResult From(string? subject, string? subjectAdditionalInfo) =>
            new AuditResult() { Subject = subject, AdditionalInfo = subjectAdditionalInfo };
    }

    public class AuditResult<T> : Result<T>, IAuditableResult
    {
        public string? Subject { get; init; }

        public string? AdditionalInfo { get; init; }

        protected AuditResult(T? data = default)
        {
            Data = data;
        }

        [Obsolete("Audit result must at least have a subject")]
        public static new AuditResult<T> From(T? data = default)
        {
            throw new InvalidOperationException("Audit result must at least have a subject");
        }

        public static AuditResult<T> From(string? subject, T? data = default) =>
            new AuditResult<T>(data) { Subject = subject };

        public static AuditResult<T> From(
            string? subject,
            string? subjectAdditionalInfo,
            T? data = default
        ) => new AuditResult<T>(data) { Subject = subject, AdditionalInfo = subjectAdditionalInfo };
    }
}
