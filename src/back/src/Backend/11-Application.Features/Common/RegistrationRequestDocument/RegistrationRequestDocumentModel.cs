using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Enums;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Common.RegistrationRequestDocument
{
    public class RegistrationRequestDocumentModel
    {
        public Guid Id { get; set; }
        public Guid? DocumentId { get; set; }
        public Guid RegistrationRequestId { get; set; }
        public RegistrationRequestDocumentType DocumentType { get; set; }

        public static RegistrationRequestDocumentModel From(RegistrationRequestDocumentDao doc)
        {
            return new RegistrationRequestDocumentModel
            {
                Id = doc.Id,
                DocumentId = doc.DocumentId,
                DocumentType = doc.RegistrationRequestDocumentType,
                RegistrationRequestId = doc.RegistrationRequestId,
            };
        }
    }
}
