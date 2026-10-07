namespace SchwarzesBrett.Models
{
    public class Attachment
    {
        public int? Id { get; set; } = null;
        public byte[]? Value { get; set; }
        public required string FileType { get; set; }
        public string? FileName { get; set; }
    }
}
