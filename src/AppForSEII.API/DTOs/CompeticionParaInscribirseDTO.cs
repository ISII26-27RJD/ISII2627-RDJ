namespace AppForSEII.Shared.CompetitionDTOs
{
    public class CompeticionParaInscribirseDTO
    {
        public CompeticionParaInscribirseDTO(int id, string name, string sportType, DateTime date, string location, double price)
        {
            Id = id;
            Name = name;
            SportType = sportType;
            Date = date;
            Location = location;
            Price = price;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string SportType { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public double Price { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CompeticionParaInscribirseDTO other &&
                   Id == other.Id &&
                   Name == other.Name &&
                   SportType == other.SportType &&
                   Date == other.Date &&
                   Location == other.Location &&
                   Price == other.Price;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, SportType, Date, Location, Price);
        }
    }
}