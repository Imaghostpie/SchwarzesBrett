namespace SchwarzesBrett.Models
{
    public class Status
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        private List<int> numbers = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private List<int> evenNumbers;

        public Status()
        {
            evenNumbers = numbers.FindAll(number => number % 2 == 0);
        }

        public bool Checknumber(int number)
        {
            foreach (int nummer in numbers)
            {

            }

            return number % 2 == 0;
        }

        Timer.tick += Checknumber()
    }
}
