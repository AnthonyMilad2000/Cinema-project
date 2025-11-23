using Cinema.Models;
using Cinema.Repositories.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Linq.Expressions;
using System.Threading;


namespace Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        private UserManager<ApplicationUser> _userManager;
        private IRepository<Cart> _cartRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Promotion> _promotionRepository;

        public CartController(UserManager<ApplicationUser> userManager, IRepository<Cart> cartRepository,
            IRepository<Movie> movieRepository, IRepository<Promotion> promotionRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _movieRepository = movieRepository;
            _promotionRepository = promotionRepository;
        }
        public async Task<IActionResult> Index(string? code = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();
            var moviesInCart = await _cartRepository.GetAsync(
                e => e.ApplicationUserId == user.Id,
                include: new Expression<Func<Cart, object>>[] { e => e.Movie }
            );

            if (code is not null)
            {
                var promotion = await _promotionRepository.GetOneAsync(e => e.Code == code);

                if (promotion is not null)
                {
                    var productInCart = moviesInCart.FirstOrDefault(e => e.MovieId == promotion.MovieId);

                    if (productInCart is not null)
                    {
                        if (promotion.IsValid && promotion.ValidTo > DateTime.UtcNow && promotion.MaxUsage > 0)
                        {
                            productInCart.Price -= productInCart.Price * (promotion.Discount / 100);
                            promotion.MaxUsage -= 1;
                            await _cartRepository.CommitAsync();
                            TempData["success-notification"] = "Applying Code Successfully";
                        }
                    }
                    else
                    {
                        TempData["error-notification"] = "Invalid Or Expired promotion";
                    }
                }
                else
                {
                    TempData["error-notification"] = "Invalid Or Expired promotion";
                }
            }

            return View(moviesInCart);
        }
        public async Task<IActionResult> AddToCart(int movieId, int count, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            var movieInDb = await _movieRepository.GetOneAsync(e => e.Id == movieId);

            if (movieInDb == null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId
            && e.ApplicationUserId == user.Id);

            if (cart is not null)
            {
                cart.Count += count;
            }
            else
            {
                await _cartRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    MovieId = movieId,
                    Count = count,
                    Price = Convert.ToDecimal(movieInDb.Price),
                }, cancellationToken);

            }


            await _cartRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> IncremntCount(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId && e.ApplicationUserId == user.Id);

            if (cart is null) return NotFound();

            cart.Count += 1;
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecremntCount(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId && e.ApplicationUserId == user.Id);

            if (cart is null) return NotFound();

            if (cart.Count > 1)
                cart.Count -= 1;
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteMovie(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId && e.ApplicationUserId == user.Id);

            if (cart is null) return NotFound();

            _cartRepository.Delete(cart);
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Pay(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepository.GetAsync(
     e => e.ApplicationUserId == user.Id,
     include: new Expression<Func<Cart, object>>[] { e => e.Movie }, 
     cancellationToken: default,
     tracked: false
 );

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/identity/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/identity/checkout/cancel",
            };

            foreach (var item in cart)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description,
                        },
                        UnitAmount = (long)item.Price * 100,
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }

    }
}
