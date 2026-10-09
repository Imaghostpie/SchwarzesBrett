namespace SchwarzesBrett.Models
{
    public class Category
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string Icon { get; set; } = "bi-three-dots";
    }
}
