using SchwarzesBrett.Models;

namespace SchwarzesBrett.ViewModels
{
    public class CreateViewModel
    {
        public ListingCreateRequest Listing { get; set; } = new();
        public ListingLookups Lookups { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
