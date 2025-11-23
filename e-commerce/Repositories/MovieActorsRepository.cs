using Cinema.Data;
using Cinema.Models;
using Cinema.Repositories.IRepository;

namespace Cinema.Repositories
{
    public class MovieActorsRepository : Repository<MovieActors>, IMovieActorsRepository
    {

        private ApplicationDbContext _context;// = new();

        public MovieActorsRepository(ApplicationDbContext context) : base(context)
        {
         _context = context;   
        }
        // Add Range

        public async Task AddRangeAsync(IEnumerable<MovieActors> movieActors, CancellationToken cancellationToken = default)
        {
            await _context.MovieActors.AddRangeAsync(movieActors, cancellationToken);
        }
    }
}
