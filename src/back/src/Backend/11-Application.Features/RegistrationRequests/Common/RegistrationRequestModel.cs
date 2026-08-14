using Application.Common.Enums;
using Application.Features.Common.Citizen;
using Application.Features.Users.Common;
using Infrastructure.Persistence.Entities;

namespace Application.Features.RegistrationRequests.Common
{
    public class RegistrationRequestModel
    {
        public Guid? Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public long? DistrictId { get; set; }
        public UserModel? Author { get; set; }
        public CitizenModel Citizen { get; set; }
        public RegistrationRequestType RequestType { get; set; }
        public ICollection<Guid>? IdentityDocumentIds { get; set; }
        public ICollection<Guid>? PhotoIds { get; set; }
        public ICollection<Guid>? ResidenceCertificateIds { get; set; }

        public RegistrationRequestDao ToDao(long districtId)
        {
            return new RegistrationRequestDao()
            {
                Id = Id ?? Guid.NewGuid(),
                ReasonForRejection = Comment,
                DistrictId = districtId,
                Citizen = Citizen?.ToDao(),
                RequestType = RequestType,
            };
        }
    }
}
