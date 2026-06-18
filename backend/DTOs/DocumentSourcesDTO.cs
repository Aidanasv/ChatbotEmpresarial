namespace backend.DTOs
{

    public class CorpusRebuildStatusResponse
    {
        public string Status { get; set; } = "idle";
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? FinishedAtUtc { get; set; }
        public string? Message { get; set; }
        public string? CorpusName { get; set; }
    }

    public class DeleteDocumentSourcesRequest
    {
        public List<string> DocumentSourceIds { get; set; } = new();
        public List<string> DocumentUris { get; set; } = new();
    }

    public class DocumentSourceResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}