namespace SchwarzesBrett.Extensions
{
    public static class FileValidator
    {
        private static readonly List<string> _allowedTypes = new List<string>()
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "application/pdf"
        };

        public const int MaxCount = 10;
        private const long _maxSize = 5 * 1024 * 1024;

        public static string? Validate(IFormFile file)
        {
            if (file.Length == 0)
                return $"{file.FileName}: Datei ist leer.";

            if (!_allowedTypes.Contains(file.ContentType))
                return $"{file.FileName}: Dateityp nicht erlaubt. Erlaubt sind JPG, PNG, GIF, WEBP und PDF.";

            if (file.Length > _maxSize)
                return $"{file.FileName}: größer als 5 MB.";

            return null;
        }



    }
}
