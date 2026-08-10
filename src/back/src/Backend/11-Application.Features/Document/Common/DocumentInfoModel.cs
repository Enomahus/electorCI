using Infrastructure.Persistence.Entities;

namespace Application.Features.Document.Common
{
    public class DocumentInfoModel
    {
        public required Guid Id { get; set; }
        public required string ContentType { get; set; }
        public required string FileName { get; set; }
        public required long Size { get; set; }

        public static DocumentInfoModel FromDao(DocumentDao dao)
        {
            return new DocumentInfoModel
            {
                Id = dao.Id,
                ContentType = dao.ContentType,
                FileName = dao.FileName,
                Size = dao.FileSize,
            };
        }
    }
}
