namespace Cinema.Repositories.IRepository
{
    public interface IMovieActorsRepository : IRepository<MovieActors>
    {
        Task AddRangeAsync(IEnumerable<MovieActors> movieActors, CancellationToken cancellationToken = default);


    }
}
