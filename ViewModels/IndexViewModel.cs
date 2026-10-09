using SchwarzesBrett.Models;

namespace SchwarzesBrett.ViewModels
{
    public class IndexViewModel
    {
        public List<ListingTile> Tiles { get; set; } = new();
        public ListingLookups Lookups { get; set; } = new();
    }
}
