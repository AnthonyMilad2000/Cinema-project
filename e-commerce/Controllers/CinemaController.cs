using Cinema.Models;
using Cinema.Models.ViewModels;
using Cinema.Repositories.IRepository;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Cinema.Controllers
{
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]

    public class CinemaController : Controller
    {
        private readonly IRepository<Cinemas> _cinemaRepository;

        public CinemaController(IRepository<Cinemas> cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken);
            indexVM m = new indexVM() { Cinemas = cinema.ToList() };
            return View(m);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCinema(Cinemas cinema, IFormFile file22, CancellationToken cancellationToken)
        {
            if (file22 is not null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file22.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets", fileName);
                using var stream = System.IO.File.Create(filePath);
                file22.CopyTo(stream);
                cinema.Image = fileName;
            }

            await _cinemaRepository.CreateAsync(cinema, cancellationToken);
            await _cinemaRepository.CommitAsync(cancellationToken);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            _cinemaRepository.Delete(cinema);
            await _cinemaRepository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var cinema = await _cinemaRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            if (cinema == null)
                return NotFound();
            return View(cinema);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Cinemas cinema, IFormFile? file22, CancellationToken cancellationToken)
        {
            var oldCinema = await _cinemaRepository.GetOne(x => x.Id == cinema.Id, cancellationToken: cancellationToken);
            if (oldCinema == null)
                return NotFound();

            if (file22 is not null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file22.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets", fileName);
                using var stream = System.IO.File.Create(filePath);
                file22.CopyTo(stream);
                oldCinema.Image = fileName;
            }

            oldCinema.Name = cinema.Name;

            await _cinemaRepository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
