using Cinema.Models;
using Cinema.Utilites;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers
{
   
        [Area("Admin")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public class UserController : Controller
        {
            private readonly RoleManager<IdentityRole> _roleManager;

            private readonly UserManager<ApplicationUser> _userManager;


        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public ViewResult Index(CancellationToken cancellationToken)
            {
                var users = _userManager.Users.AsNoTracking().AsQueryable();

                //

                return View(users.AsEnumerable());
            }

           
            public async Task<IActionResult> LockUnLock(string id)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user is null) return NotFound();

                if (await _userManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE))
                {
                    TempData["error-notification"] = "You Can not Block Super Admin Account";
                }
                else
                {
                    user.LockoutEnabled = !user.LockoutEnabled;

                    if (!user.LockoutEnabled)
                        user.LockoutEnd = DateTime.UtcNow.AddMonths(1);
                    else
                        user.LockoutEnd = null;

                    await _userManager.UpdateAsync(user);
                }

                return RedirectToAction(nameof(Index));
            }
            [HttpGet]
            public IActionResult Create()
            {
                var vm = new ApplicationUserVM
                {
                    Roles = _roleManager.Roles.Select(r => new SelectListItem
                    {
                        Value = r.Name,
                        Text = r.Name
                    })
                };

                return View(vm);
            }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                });

                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Name,
                Email = vm.Email,
                Name = vm.Name,
                EmailConfirmed = true  
            };

            var result = await _userManager.CreateAsync(user, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                vm.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                });

                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, vm.Role);

            TempData["success-notification"] = "User created successfully";

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var vm = new ApplicationUserVM
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList()
            };

            var rolesForUser = await _userManager.GetRolesAsync(user);
            vm.Role = rolesForUser.FirstOrDefault();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUserVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email; 
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                model.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();

                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            TempData["success-notification"] = "User updated successfully";

            return RedirectToAction("Index");
        }


    }
}
