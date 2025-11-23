using Cinema.Models;
using Cinema.Repositories.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Cinema.Services
{
    public class AccountHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _otpRepository;

        public AccountHelper(UserManager<ApplicationUser> userManager,
                             IEmailSender emailSender,
                             IRepository<ApplicationUserOTP> otpRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _otpRepository = otpRepository;
        }

        public async Task<ApplicationUser?> FindByUserNameOrEmailAsync(string usernameOrEmail)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                       ?? await _userManager.FindByEmailAsync(usernameOrEmail);
            return user;
        }

        public async Task SendEmailConfirmationAsync(ApplicationUser user, string baseUrl, string subject = "Confirm Your Email!")
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = $"{baseUrl}?token={Uri.EscapeDataString(token)}&id={user.Id}";
            await _emailSender.SendEmailAsync(user.Email, $"Cinema - {subject}",
                $"<h1>Please Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");
        }

        public async Task<string> CreateOTPAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var otp = new Random().Next(1000, 9999).ToString();
            await _otpRepository.CreateAsync(new ApplicationUserOTP
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                OTP = otp,
                IsValid = true,
                CreateAt = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddMinutes(30)
            }, cancellationToken);
            await _otpRepository.CommitAsync(cancellationToken);
            return otp;
        }
        public async Task SendEmailAsync(ApplicationUser user, string subject, string message)
        {
            await _emailSender.SendEmailAsync(user.Email!, $"Cinema - {subject}", message);
        }

        public async Task<ApplicationUserOTP?> ValidateOTPAsync(string userId)
        {
            return await _otpRepository.GetOneAsync(e => e.ApplicationUserId == userId && e.IsValid && e.ValidTo > DateTime.UtcNow);
        }
    }
}
