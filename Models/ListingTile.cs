
namespace SchwarzesBrett.Models
{
    public class ListingTile
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public decimal? Price { get; set; }
        public string? StatusName { get; set; }
        public required string CategoryName { get; set; }
        public required int CategoryID { get; set; }
        public required string PriceCategoryName { get; set; }
        public required string ListingTypeName { get; set; }
        public required int ListingTypeId { get; set; }
        public int? TitleImageId { get; set; }
        public int StatusId { get; set; }
        public DateTime ValidUntil { get; set; }

        public int DaysLeft => (ValidUntil.Date - DateTime.Today).Days;
        public required DateTime CreatedAt { get; set; }
        public string AgeText {
            get
            {
                TimeSpan alter = DateTime.Now - CreatedAt;
                int days = alter.Days;

                if (days == 0) return "Heute";
                if (days == 1) return "Gestern";
                if (days < 30) return $"vor {days} Tagen";

                int months = (DateTime.Now.Year - CreatedAt.Year) * 12 + (DateTime.Now.Month - CreatedAt.Month);
                if (DateTime.Now.Day < CreatedAt.Day) months--;     
                months = Math.Max(months, 1);              

                if (months < 12)
                    return months == 1 ? "vor 1 Monat" : $"vor {months} Monaten";

                int years = months / 12;
                return years == 1 ? "vor 1 Jahr" : $"vor {years} Jahren";
            }
        }




    }
}
