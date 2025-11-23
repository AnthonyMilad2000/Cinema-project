namespace Cinema.Models
{
    public class MovieImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
