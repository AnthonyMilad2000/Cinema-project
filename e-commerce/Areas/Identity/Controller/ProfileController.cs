using Cinema.Models;
using Cinema.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce518.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            //var userVM = new ApplicationUserVM()
            //{
            //    Address = user.Address,
            //    Email = user.Email!,
            //    Name = user.Name,
            //    PhoneNumber = user.PhoneNumber
            //};

            var userVM = user.Adapt<ApplicationUserVM>();

            return View(userVM);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM userVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            user.Name = userVM.Name;
            user.PhoneNumber = userVM.PhoneNumber;
            user.Address = userVM.Address;

            //var adabtedUser = userVM.Adapt<ApplicationUser>();

            await _userManager.UpdateAsync(user);

            TempData["success-notification"] = "Update Profile";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(ApplicationUserVM userVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null) return NotFound();

            if (userVM.CurrentPassword is null || userVM.NewPassword is null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, userVM.CurrentPassword, userVM.NewPassword);

            if (!result.Succeeded)
                TempData["error-notification"] = String.Join(", ", result.Errors.Select(e => e.Description));
            else
                TempData["success-notification"] = "Update Password Profile";

            return RedirectToAction(nameof(Index));
        }
    }
}