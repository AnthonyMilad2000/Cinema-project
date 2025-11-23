using Cinema.Models;
using Cinema.Models.ViewModels;
using Cinema.Repositories.IRepository;
using Cinema.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AccountHelper _accountHelper;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 AccountHelper accountHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountHelper = accountHelper;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            var user = new ApplicationUser()
            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                    ModelState.AddModelError(string.Empty, item.Code);
                return View(registerVM);
            }

            await _accountHelper.SendEmailConfirmationAsync(user,
                Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity" }, Request.Scheme));

            TempData["success-notification"] = "Send Email Successfully";

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                TempData["error-notification"] = "Invalid Or Expired Token";
            else
                TempData["success-notification"] = "Confirm Your Email Successfully";

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _accountHelper.FindByUserNameOrEmailAsync(loginVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email OR Password");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Too many attemps, try again after 5 min");
                else if (result.IsNotAllowed)
                    ModelState.AddModelError(string.Empty, "Please Confirm Your Email First!!");
                else
                    ModelState.AddModelError(string.Empty, "Invalid User Name / Email OR Password");

                return View(loginVM);
            }

            TempData["success-notification"] = "Login Successfully";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _accountHelper.FindByUserNameOrEmailAsync(resendEmailConfirmationVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(resendEmailConfirmationVM);
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Already Confirmed!!!");
                return View(resendEmailConfirmationVM);
            }

            await _accountHelper.SendEmailConfirmationAsync(user,
                Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity" }, Request.Scheme),
                "Resend Confirm Your Email!");

            TempData["success-notification"] = "Send Email Successfully";
            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _accountHelper.FindByUserNameOrEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(forgetPasswordVM);
            }

            var otpCount = (await _accountHelper.ValidateOTPAsync(user.Id)) != null ? 1 : 0;
            if (otpCount > 5)
            {
                ModelState.AddModelError(string.Empty, "Pleas Try Again Later. Too Many Attemps");
                return View(forgetPasswordVM);
            }

            var otp = await _accountHelper.CreateOTPAsync(user, cancellationToken);

            await _accountHelper.SendEmailAsync(user,
             "Forget Password!",
             $"Use this OTP: {otp} To Validate Your Account. Don't share it.");


            TempData["success-notification"] = "Send OTP Your Email";
            TempData["From-ForgetPassword"] = Guid.NewGuid().ToString();

            return RedirectToAction("ValidateOTP", new { userId = user.Id });
        }

        public IActionResult ValidateOTP(string userId)
        {
            if (TempData["From-ForgetPassword"] is null)
                return NotFound();

            return View(new ValidateOTP() { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTP validateOTP)
        {
            if (!ModelState.IsValid)
                return View(validateOTP);

            var validOTP = await _accountHelper.ValidateOTPAsync(validateOTP.UserId);

            if (validOTP is null)
            {
                TempData["error-notification"] = "Invalid OTP";
                return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTP.UserId });
            }

            TempData["From-ValidateOTP"] = Guid.NewGuid().ToString();
            return RedirectToAction("NewPassword", new { userId = validateOTP.UserId });
        }

        public IActionResult NewPassword(string userId)
        {
            if (TempData["From-ValidateOTP"] is null)
                return NotFound();

            return View(new NewPasswordVM() { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(newPasswordVM);

            var user = await _userManager.FindByIdAsync(newPasswordVM.UserId);

            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction(nameof(NewPassword), new { userId = newPasswordVM.UserId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

            TempData["success-notification"] = "Change Password Successfully";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    Random random = new Random();
                    int r = random.Next(1000, 9999);
                    user = new ApplicationUser
                    {
                        UserName = username.Replace(" ", "") + r.ToString(),
                        Email = email,
                        EmailConfirmed = true
                    };
                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Error creating user.");
                        return RedirectToAction(nameof(Login));
                    }
                }

                var existingLogins = await _userManager.GetLoginsAsync(user);
                var hasProviderLogin = existingLogins.Any(l => l.LoginProvider == info.LoginProvider);

                if (!hasProviderLogin)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Error linking external login.");
                        return RedirectToAction(nameof(Login));
                    }
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? "/Home/Index");
            }

            return RedirectToAction(nameof(Login));
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData["success-notification"] = "Logout Successfully";

            return RedirectToAction("Login");
        }


    }
}
