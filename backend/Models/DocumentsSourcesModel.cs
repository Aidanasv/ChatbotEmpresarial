namespace backend.Models
{
    public class DocumentSource
    {
        public string Id { get; set; } = string.Empty; // Identificador único del documento
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public string Name { get; set; } = string.Empty;
        public TypeOfDocument DocumentType { get; set; } = TypeOfDocument.pdf;
        public string Uri { get; set; } = string.Empty; // Ruta o URL donde se almacena el documento
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}