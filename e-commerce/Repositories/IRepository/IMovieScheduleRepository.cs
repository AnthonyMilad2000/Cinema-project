using Cinema.Models;

namespace Cinema.Repositories.IRepository
{
    public interface IMovieScheduleRepository : IRepository<MovieSchedule>
    {
        Task AddRangeAsync(IEnumerable<MovieSchedule> schedules, CancellationToken cancellationToken = default);


    }
}
