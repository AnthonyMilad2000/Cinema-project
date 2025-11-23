using Cinema.Models;

public class MovieActors
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public Movie Movie { get; set; }
    public int ActorId { get; set; }
    public Actors Actor { get; set; }
}
