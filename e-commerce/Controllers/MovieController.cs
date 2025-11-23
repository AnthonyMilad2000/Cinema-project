using Cinema.Data;
using Cinema.Models;
using Cinema.Models.ViewModels;
using Cinema.Repositories;
using Cinema.Repositories.IRepository;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cinema.Controllers
{
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]

    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinemas> _cinemaRepository;
        private readonly IRepository<Actors> _actorRepository;
        private readonly IRepository<MovieImage> _movieImageRepository;
        private readonly IMovieScheduleRepository _movieSchedulesRepository;
        private readonly IMovieActorsRepository _movieActorsRepository;

        // private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();
        //private IRepository<Movie> _movieRepository = new Repository<Movie>();
        //private IRepository<Category> _categoryRepository = new Repository<Category>();
        //private IRepository<Cinemas> _cinemaRepository = new Repository<Cinemas>();
        //private IRepository<Actors> _actorRepository = new Repository<Actors>();
        //private IRepository<MovieImage> _movieImageRepository = new Repository<MovieImage>();
        //private IRepository<MovieSchedule> MovieScheduleRepository = new Repository<MovieSchedule>();
        //private IMovieScheduleRepository MovieSchedulesRepository = new MovieSchedulesRepository();
        //private IMovieActorsRepository MovieActorsRepository = new MovieActorsRepository();
        public MovieController(
             IRepository<Movie> movieRepository,
             IRepository<Category> categoryRepository,
             IRepository<Cinemas> cinemaRepository,
             IRepository<Actors> actorRepository,
             IRepository<MovieImage> movieImageRepository,
             IMovieScheduleRepository movieSchedulesRepository,
             IMovieActorsRepository movieActorsRepository)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _actorRepository = actorRepository;
            _movieImageRepository = movieImageRepository;
            _movieSchedulesRepository = movieSchedulesRepository;
            _movieActorsRepository = movieActorsRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetAsync(
                include: new Expression<Func<Movie, object>>[]
                {
            m => m.SubImages,
            m => m.Schedules,
            m => m.MovieActors
                },
                cancellationToken: cancellationToken
            );

            indexVM m = new indexVM() { Movie = movies.ToList() };
            return View(m);
        }



        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAsync();
            ViewBag.Cinemas = await _cinemaRepository.GetAsync();
            ViewBag.Actors = await _actorRepository.GetAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Movie movie,
            IFormFile mainImage,
            List<DateTime> ShowTimes,
            List<IFormFile> subImages,
            List<int> ActorIds,
            CancellationToken cancellationToken)
        {
            if (mainImage != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(mainImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets", fileName);
                using (var stream = System.IO.File.Create(path))
                {
                    await mainImage.CopyToAsync(stream, cancellationToken);
                }
                movie.MainImage = fileName;
            }

            if (subImages != null && subImages.Any())
            {
                movie.SubImages = new List<MovieImage>();
                foreach (var img in subImages)
                {
                    var subFileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets", subFileName);
                    using (var stream = System.IO.File.Create(subPath))
                    {
                        await img.CopyToAsync(stream, cancellationToken);
                    }
                    movie.SubImages.Add(new MovieImage { ImagePath = subFileName });
                }
            }

            await _movieRepository.CreateAsync(movie, cancellationToken);
            await _movieRepository.CommitAsync(cancellationToken);

            if (ShowTimes?.Any() == true)
            {
                var movieSchedules = ShowTimes.Select(t => new MovieSchedule
                {
                    ShowTime = t,
                    MovieId = movie.Id
                }).ToList();

                await _movieSchedulesRepository.AddRangeAsync(movieSchedules, cancellationToken);
            }

            if (ActorIds?.Any() == true)
            {
                var movieActors = ActorIds.Select(id => new MovieActors
                {
                    MovieId = movie.Id,
                    ActorId = id
                }).ToList();

                await _movieActorsRepository.AddRangeAsync(movieActors, cancellationToken);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetOne(
     expression: x => x.Id == id,
     include: new Expression<Func<Movie, object>>[]
     {
        m => m.SubImages,
        m => m.Schedules,
        m => m.MovieActors
     },
     cancellationToken: cancellationToken
 );


            if (movie == null)
                return NotFound();

            movie.ActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList();

            ViewBag.Categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
            ViewBag.Cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken);
            ViewBag.Actors = await _actorRepository.GetAsync(cancellationToken: cancellationToken);

            return View(movie);
        }

        [HttpPost]

        public async Task<IActionResult> Edit(Movie movie, IFormFile? mainImage, List<IFormFile>? subImages, List<DateTime>? ShowTimes, List<int>? ActorIds, CancellationToken cancellationToken)
        {
            var oldMovie = await _movieRepository.GetOne(
                expression: x => x.Id == movie.Id,
                include: new Expression<Func<Movie, object>>[]
                {
                    m => m.SubImages,
                    m => m.Schedules,
                    m => m.MovieActors
                },
                cancellationToken: cancellationToken
            );

            if (oldMovie == null)
                return NotFound();

            oldMovie.Name = movie.Name;
            oldMovie.Description = movie.Description;
            oldMovie.Price = movie.Price;
            oldMovie.Status = Request.Form["Status"] == "true";
            oldMovie.CategoryId = movie.CategoryId;
            oldMovie.CinemaId = movie.CinemaId;

            if (mainImage != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(mainImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets", fileName);
                using (var stream = System.IO.File.Create(path))
                {
                    mainImage.CopyTo(stream);
                }
                oldMovie.MainImage = fileName;
            }

            if (subImages != null && subImages.Any())
            {
                foreach (var img in subImages)
                {
                    var subFileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets", subFileName);
                    using (var stream = System.IO.File.Create(subPath))
                    {
                        img.CopyTo(stream);
                    }
                    oldMovie.SubImages.Add(new MovieImage { ImagePath = subFileName });
                }
            }

            oldMovie.Schedules.Clear();
            if (ShowTimes != null && ShowTimes.Any())
            {
                oldMovie.Schedules = ShowTimes.Select(t => new MovieSchedule { ShowTime = t }).ToList();
            }

            oldMovie.MovieActors.Clear();
            if (ActorIds != null && ActorIds.Any())
            {
                oldMovie.MovieActors = ActorIds.Select(id => new MovieActors { ActorId = id }).ToList();
            }

            await _movieRepository.CommitAsync(cancellationToken: cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubImage(int id, CancellationToken cancellationToken)
        {
            var subImage = await _movieImageRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            if (subImage == null)
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets", subImage.ImagePath);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _movieImageRepository.Delete(subImage);
            await _movieImageRepository.CommitAsync(cancellationToken: cancellationToken);

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteShowTime(int id, CancellationToken cancellationToken)
        {
            var show = await _movieImageRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            if (show == null)
                return Json(new { success = false });

            _movieImageRepository.Delete(show);
            await _movieImageRepository.CommitAsync(cancellationToken: cancellationToken);

            return Json(new { success = true });
        }
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            _movieRepository.Delete(movie);
            await _movieRepository.CommitAsync(cancellationToken: cancellationToken);
            return RedirectToAction(nameof(Index));
        }


    }
}
