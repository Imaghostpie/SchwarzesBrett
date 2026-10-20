namespace SchwarzesBrett.Models
{
    public class ListingCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ListingTypeId { get; set; }
        public int CategoryId { get; set; }
        public int PriceCategoryId { get; set; }
        public decimal? Price { get; set; }
        public string? ContactPhone { get; set; }
        public string ContactMail { get; set; } = null!;
        public List<IFormFile> Attachments { get; set; } = new();
        public int TitleImageIndex { get; set; }
        public DateTime ValidFrom { get; set; } = DateTime.Today;
        public DateTime? ValidUntil { get; set; } = DateTime.Today.AddMonths(1);
        public bool IsPermanent { get; set; }
        public bool UseCustomPeriod { get; set; }
    }
}
