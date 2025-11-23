using Cinema.Data;
using Cinema.Models;
using Cinema.Repositories.IRepository;
using System.Threading;
using System.Threading.Tasks;
namespace Cinema.Repositories
{
    public class MovieSchedulesRepository : Repository<MovieSchedule>, IMovieScheduleRepository
    {
        private ApplicationDbContext _context;// = new();

        public MovieSchedulesRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        // Add Range

        public async Task AddRangeAsync(IEnumerable<MovieSchedule> schedules, CancellationToken cancellationToken = default)
        {
            await _context.MovieSchedules.AddRangeAsync(schedules, cancellationToken);
        }
    }
}
