using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Enums;
using Application.Features.Common.Citizen;
using Application.Features.Common.RegistrationRequestDocument;
using Application.Features.RegistrationRequests.Common;
using Application.Features.Users.Common;
using Infrastructure.Persistence.Entities;

namespace Application.Features.RegistrationRequests.GetRegistrationRequest
{
    public class GetRegistrationRequestResponse : RegistrationRequestModel
    {
        public DateTimeOffset SubmittedAt { get; set; }
        public RegistrationStatus Status { get; set; }
        public string DistrictName { get; set; }
        public Guid? AuthorId { get; set; }
        public List<RegistrationRequestDocumentModel> RequestDocuments { get; set; } = [];

        public static GetRegistrationRequestResponse From(
            RegistrationRequestDao dao,
            UserDao user,
            DateTimeOffset now
        )
        {
            return new()
            {
                Id = dao.Id,
                Reference = dao.Reference,
                SubmittedAt = dao.SubmissionDate,
                Status = dao.Status,
                Comment = dao.ReasonForRejection,
                DistrictId = dao.DistrictId,
                DistrictName = dao.District.Wording,
                AuthorId = dao.AuthorId,
                Author = UserModel.FromDao(user, now),
                Citizen = CitizenModel.FromDao(dao.Citizen),
                RequestDocuments =
                [
                    .. dao.RegistrationRequestDocuments.Select(RegistrationRequestDocumentModel.From),
                ],
            };
        }
    }
}
