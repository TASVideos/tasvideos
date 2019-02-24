using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Services.Email;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Account
{
	[AllowAnonymous]
	public class RegisterModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IEmailService _emailService;
		private readonly ExternalMediaPublisher _publisher;

		public RegisterModel(
			ApplicationDbContext db,
			UserManager userManager,
			SignInManager<User> signInManager,
			IEmailService emailService,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_userManager = userManager;
			_signInManager = signInManager;
			_emailService = emailService;
			_publisher = publisher;
		}

		[FromQuery]
		public string ReturnUrl { get; set; }

		[BindProperty]
		[Display(Name = "Time Zone")]
		public string SelectedTimeZone { get; set; }

		[BindProperty]
		[Required]
		[StringLength(256)]
		[Display(Name = "User Name")]
		public string UserName { get; set; }

		[BindProperty]
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[BindProperty]
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[BindProperty]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[BindProperty]
		[Display(Name = "Location")]
		[StringLength(256)]
		public string From { get; set; }

		public async Task<IActionResult> OnPost()
		{
			if (ModelState.IsValid)
			{
				var user = new User
				{
					UserName = UserName,
					Email = Email,
					TimeZoneId = SelectedTimeZone,
					From = From
				};
				var result = await _userManager.CreateAsync(user, Password);
				if (result.Succeeded)
				{
					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
					await _emailService.EmailConfirmation(Email, callbackUrl);

					await AddStandardRoles(user.Id);
					await _userManager.AddUserPermissionsToClaims(user);
					await _signInManager.SignInAsync(user, isPersistent: false);

					_publisher.SendUserManagement($"New User joined! {user.UserName}", "", $"{BaseUrl}/Users/Profile/{user.UserName}");
					return RedirectToLocal(ReturnUrl);
				}

				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
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
