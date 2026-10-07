namespace SchwarzesBrett.Models
{
    public class ListingEditRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ListingTypeId { get; set; }
        public int CategoryId { get; set; }
        public int PriceCategoryId { get; set; }
        public decimal? Price { get; set; }
        public string? ContactPhone { get; set; }
        public string ContactMail { get; set; } = null!;
    }
}
