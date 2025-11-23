using Cinema.Data;
using Cinema.Models;
using Cinema.Models.ViewModels;
using Cinema.Repositories;
using Cinema.Repositories.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using Cinema.Utilites;

namespace Cinema.Controllers
{
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]

    public class ActorController : Controller
    {
        private readonly IRepository<Actors> _actorRepository;

        //private ApplicationDbContext _dbContext = new ApplicationDbContext();
        //private IRepository<Actors> _actorRepository = new Repository<Actors>();
        public ActorController(IRepository<Actors> actorRepository)
        {
            _actorRepository = actorRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var actors = await _actorRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);

            indexVM m = new indexVM() {
                Actors = actors.ToList() 
            };
            return View(m);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateActor(Actors actor, IFormFile file22, CancellationToken cancellationToken)
        {

            if (file22 is not null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file22.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file22.CopyTo(stream);
                }

                actor.Image = fileName;
            }
            await _actorRepository.CreateAsync(actor, cancellationToken: cancellationToken);
            await _actorRepository.CommitAsync(cancellationToken: cancellationToken);
            return RedirectToAction("index");
        }


        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var actor =await _actorRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            _actorRepository.Delete(actor);
             await _actorRepository.CommitAsync(cancellationToken: cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var actor = await _actorRepository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            if (actor == null)
                return NotFound();

            return View(actor);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Actors actor, IFormFile? file22, CancellationToken cancellationToken)
        {
            var oldActor = await _actorRepository.GetOne(x => x.Id == actor.Id, cancellationToken: cancellationToken);
            if (oldActor == null)
                return NotFound();

            if (file22 is not null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file22.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\assets", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    file22.CopyTo(stream);
                }
                oldActor.Image = fileName;
            }

            oldActor.Name = actor.Name;

            await _actorRepository.CommitAsync(cancellationToken: cancellationToken);
            return RedirectToAction(nameof(Index));
        }

    }
}
