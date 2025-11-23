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

    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _repository;

        public CategoryController(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var categories = await _repository.GetAsync(tracked: false, cancellationToken: cancellationToken);
            indexVM m = new indexVM() { Category = categories.ToList() };
            return View(m);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string Name, CancellationToken cancellationToken)
        {
            var category = new Category { Title = Name };
            await _repository.CreateAsync(category, cancellationToken);
            await _repository.CommitAsync(cancellationToken);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _repository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            if (category == null)
                return NotFound();

            _repository.Delete(category);
            await _repository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var category = await _repository.GetOne(x => x.Id == id, cancellationToken: cancellationToken);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category, CancellationToken cancellationToken)
        {
            var oldCategory = await _repository.GetOne(x => x.Id == category.Id, cancellationToken: cancellationToken);
            if (oldCategory == null)
                return NotFound();

            oldCategory.Title = category.Title;

            _repository.Update(oldCategory);
            await _repository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
