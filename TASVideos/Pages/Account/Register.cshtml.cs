using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models.ValidationAttributes;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	[IpBanCheck]
	public class RegisterModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IEmailService _emailService;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IReCaptchaService _reCaptchaService;
		private readonly IWebHostEnvironment _env;

		public RegisterModel(
			ApplicationDbContext db,
			UserManager userManager,
			SignInManager<User> signInManager,
			IEmailService emailService,
			ExternalMediaPublisher publisher,
			IReCaptchaService reCaptchaService,
			IWebHostEnvironment env)
		{
			_db = db;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
			_publisher = publisher;
			_reCaptchaService = reCaptchaService;
			_env = env;
		}

		[FromQuery]
		public string? ReturnUrl { get; set; }

		[BindProperty]
		[Display(Name = "Time Zone")]
		public string? SelectedTimeZone { get; set; }

		[BindProperty]
		[Required]
		[StringLength(256)]
		[Display(Name = "User Name")]
		public string UserName { get; set; } = "";

		[RegularExpression(@"[^+]+", ErrorMessage = "Email pattern not allowed.")]
		[BindProperty]
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; } = "";

		[BindProperty]
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = "";

		[BindProperty]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		public string ConfirmPassword { get; set; } = "";

		[BindProperty]
		[Display(Name = "Location")]
		[StringLength(256)]
		public string? From { get; set; }

		[BindProperty]
		[Required]
		[MustBeTrue]
		[Display(Name = "By checking the box below, you certify you are 13 years of age or older")]
		public bool COPPA { get; set; }

		public async Task<IActionResult> OnPost()
		{
			if (Password != ConfirmPassword)
			{
				ModelState.AddModelError(nameof(ConfirmPassword), "The password and confirmation password do not match.");
			}

			string encodedResponse = Request.Form["g-recaptcha-response"];
			bool isCaptchaValid = await _reCaptchaService.Verify(encodedResponse);

			if (!_env.IsDevelopment() && !isCaptchaValid)
			{
				ModelState.AddModelError("", "TASVideos prefers human users.  If you believe you have received this message in error, please contact admin@tasvideos.org");
			}

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var disallows = await _db.UserDisallows.ToListAsync();
			foreach (var disallow in disallows)
			{
				var regex = new Regex(disallow.RegexPattern);
				if (regex.IsMatch(UserName))
				{
					ModelState.AddModelError(nameof(UserName), "The username is not allowed.");
					break;
				}
			}

			if (ModelState.IsValid)
			{
				var user = new User
				{
					UserName = UserName,
					Email = Email,
					TimeZoneId = SelectedTimeZone ?? TimeZoneInfo.Utc.Id,
					From = From
				};
				var result = await _userManager.CreateAsync(user, Password);
				if (result.Succeeded)
				{
					await AddStandardRoles(user.Id);
					await _userManager.AddUserPermissionsToClaims(user);

					if (_userManager.Options.SignIn.RequireConfirmedEmail)
					{
						var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
						var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
						await _emailService.EmailConfirmation(Email, callbackUrl);
						return RedirectToPage("EmailConfirmationSent");
					}

					await _signInManager.SignInAsync(user, isPersistent: false);
					_publisher.SendUserManagement($"New User joined! {user.UserName}", "", $"Users/Profile/{user.UserName}", user.UserName);
					return RedirectToLocal(ReturnUrl);
				}

				AddErrors(result);
			}

			return Page();
		}

		public async Task AddStandardRoles(int userId)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == userId);
			var roles = await _db.Roles
				.ThatAreDefault()
				.ToListAsync();

			foreach (var role in roles)
			{
				var userRole = new UserRole
				{
					UserId = user.Id,
					RoleId = role.Id
				};
				_db.UserRoles.Add(userRole);
				user.UserRoles.Add(userRole);
			}

			await _db.SaveChangesAsync();
		}
	}
}
