using SchwarzesBrett.Models;

namespace SchwarzesBrett.ViewModels
{
    public class EditViewModel
    {
        public ListingEditRequest Listing { get; set; } = new();
        public ListingLookups Lookups { get; set; } = new();
        public List<Attachment>? Attachments { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
