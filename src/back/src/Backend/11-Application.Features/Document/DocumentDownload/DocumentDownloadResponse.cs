namespace Application.Features.Document.DocumentDownload
{
    public class DocumentDownloadResponse
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required Stream Content { get; set; }
    }
}
