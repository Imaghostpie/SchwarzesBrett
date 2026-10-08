namespace SchwarzesBrett.Models
{
    public class Attachment
    {
        public int? Id { get; set; } = null;
        public byte[]? Value { get; set; }
        public required string FileType { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText
        {
            get
            {
                if (FileSize >= 1024 * 1024)
                    return $"{FileSize / (1024.0 * 1024):0.#} MB";

                return $"{FileSize / 1024.0:0} KB";
            }
        }
    }
}
