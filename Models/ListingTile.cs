
namespace SchwarzesBrett.Models
{
    public class ListingTile
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public decimal? Price { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string StatusName { get; set; }
        public required string CategoryName { get; set; }
        public required string PriceCategoryName { get; set; }
        public required string ListingTypeName { get; set; }
        public int? TitleImageId { get; set; }


    }
}
