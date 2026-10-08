namespace SchwarzesBrett.Models
{
    public class Listing
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public required ListingType ListingType { get; set; }
        public required Category Category { get; set; }
        public required PriceCategory PriceCategory { get; set; }
        public decimal? Price { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public string? ContactPhone { get; set; }
        public required string ContactMail { get; set; }
        public required DateTime ValidUntil { get; set; }
        public required Status Status { get; set; }
        public List<Attachment>? Attachments { get; set; } = new();
        public string CreatorName
        {
            get
            {
                int index = CreatedBy.LastIndexOf('\\');
                return index >= 0 ? CreatedBy[(index + 1)..] : CreatedBy;
            }
        }
        public string CreatorInitials
        {
            get
            {
                string name = CreatorName;
                return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
            }
        }

        public int DaysLeft => (ValidUntil.Date - DateTime.Today).Days;
    }
}
