using Cinema.Models;
using Cinema.Models.ViewModels;
using Cinema.Repositories.IRepository;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;

        public HomeController(IRepository<Movie> movieRepository)
        {
            _movieRepository = movieRepository;
        }
        public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
        {
            int pageSize = 8;

            var movies = await _movieRepository.GetAsync(
                include: new Expression<Func<Movie, object>>[]
                {
            m => m.SubImages,
            m => m.Schedules,
            m => m.MovieActors
                },
                cancellationToken: cancellationToken
            );

            var moviesList = movies.ToList();

            var pagedMovies = moviesList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new IndexVM
            {
                Movie = pagedMovies,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)moviesList.Count / pageSize)
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var movies = await _movieRepository.GetAsync(
                include: new Expression<Func<Movie, object>>[]
                {
                m => m.SubImages,
                m => m.Schedules,
                m => m.MovieActors,
                m => m.Cinema
                },
                cancellationToken: cancellationToken
            );

            var movie = movies.FirstOrDefault(m => m.Id == id);

            if (movie == null) return NotFound();

            var vm = new MovieDetailsVM
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Price = movie.Price,
                Status = movie.Status,
                MainImage = movie.MainImage,
                Schedules = movie.Schedules.ToList(),
                CinemasCount = 1,
                CinemaName = movie.Cinema?.Name,
                SubImages = movie.SubImages.ToList()
            };

            return View(vm);
        }

    }


}


