namespace SchwarzesBrett.Models
{
    public class ListingLookups
    {
        public List<Category> Categories { get; set; } = new();
        public List<ListingType> ListingTypes { get; set; } = new();
        public List<PriceCategory> PriceCategories { get; set; } = new();
        public List<Status> Statuses { get; set; } = new();
    }
}
