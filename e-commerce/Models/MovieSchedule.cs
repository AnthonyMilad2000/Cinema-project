namespace Cinema.Models
{
    public class MovieSchedule
    {
        public int Id { get; set; }

        public DateTime ShowTime { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
