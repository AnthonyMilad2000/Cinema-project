namespace Cinema.Models { 

public class Movie
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public bool Status { get; set; }
    public string MainImage { get; set; }

    public ICollection<MovieImage> SubImages { get; set; } = new List<MovieImage>();
    public ICollection<Actors> Actors { get; set; } = new List<Actors>();
    public ICollection<MovieActors> MovieActors { get; set; }

        public int CategoryId { get; set; }
    public Category Category { get; set; }

    public int CinemaId { get; set; }
    public Cinemas Cinema { get; set; }

    public ICollection<MovieSchedule> Schedules { get; set; } = new List<MovieSchedule>();
        public List<int> ActorIds { get; set; } = new();

    }
}