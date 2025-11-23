using Cinema.Models;
using Cinema.Repositories.IRepository;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}, {SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class PromotionController : Controller
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Movie> _movieRepository;

        public PromotionController(
            IRepository<Promotion> promotionRepository,
            IRepository<Movie> movieRepository)
        {
            _promotionRepository = promotionRepository;
            _movieRepository = movieRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var promotions = await _promotionRepository.GetAsync(
             include: new Expression<Func<Promotion, object>>[]
             {
                p => p.Movie
             },
             cancellationToken: cancellationToken,
             tracked: false
         );

            return View(promotions.ToList());
        }


        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);

            ViewBag.MovieList = movies.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            return View(new Promotion());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion promotion, CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);
            ViewBag.MovieList = movies.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                Console.WriteLine("ModelState Errors: " + errors);
                return View("Create", promotion);
            }

            await _promotionRepository.CreateAsync(promotion, cancellationToken);
            await _promotionRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id,
            CancellationToken cancellationToken)
        {
            var promotion = await _promotionRepository.GetOne(
                x => x.Id == id,
                cancellationToken: cancellationToken);

            if (promotion == null) return NotFound();

            _promotionRepository.Delete(promotion);
            await _promotionRepository.CommitAsync(cancellationToken: cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var promotion = await _promotionRepository.GetOne(
                x => x.Id == id,
                cancellationToken: cancellationToken);

            if (promotion == null) return NotFound();

            var movies = await _movieRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);
            ViewBag.MovieList = movies.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Promotion promotion, CancellationToken cancellationToken)
        {
            var oldPromotion = await _promotionRepository.GetOne(
                x => x.Id == promotion.Id,
                cancellationToken: cancellationToken);

            if (oldPromotion == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var movies = await _movieRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);
                ViewBag.MovieList = movies.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                }).ToList();

                return View(promotion);
            }

            oldPromotion.Code = promotion.Code;
            oldPromotion.Discount = promotion.Discount;
            oldPromotion.IsValid = promotion.IsValid;
            oldPromotion.ValidTo = promotion.ValidTo;
            oldPromotion.MaxUsage = promotion.MaxUsage;
            oldPromotion.MovieId = promotion.MovieId;

            await _promotionRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

    }


}
