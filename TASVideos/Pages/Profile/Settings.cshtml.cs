using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Pages.Profile.Models;
using TASVideos.Services;
using TASVideos.Services.Email;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class SettingsModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly IEmailService _emailService;
		private readonly ApplicationDbContext _db;

		public SettingsModel(
			UserManager userManager,
			IEmailService emailService,
			ApplicationDbContext db)
		{
			_userManager = userManager;
			_emailService = emailService;
			_db = db;
		}

		[TempData]
		public string StatusMessage { get; set; }

		[BindProperty]
		public ProfileSettingsModel Settings { get; set; }

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Settings = new ProfileSettingsModel
			{
				Username = user.UserName,
				Email = user.Email,
				TimeZoneId = user.TimeZoneId,
				IsEmailConfirmed = user.EmailConfirmed,
				PublicRatings = user.PublicRatings,
				StatusMessage = StatusMessage,
				From = user.From,
				Signature = user.Signature,
				Avatar = user.Avatar,
				Roles = await _db.Users
					.Where(u => u.Id == user.Id)
					.SelectMany(u => u.UserRoles)
					.Select(ur => ur.Role)
					.Select(r => new RoleBasicDisplay
					{
						Id = r.Id,
						Name = r.Name,
						Description = r.Description
					})
					.ToListAsync()
			};
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);

			var email = user.Email;
			if (Settings.Email != email)
			{
				var setEmailResult = await _userManager.SetEmailAsync(user, Settings.Email);

				if (!setEmailResult.Succeeded)
				{
					throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
				}
			}

			user.TimeZoneId = Settings.TimeZoneId;
			user.PublicRatings = Settings.PublicRatings;
			user.From = Settings.From;
			user.Signature = Settings.Signature;
			user.Avatar = Settings.Avatar;
			await _db.SaveChangesAsync();

			StatusMessage = "Your profile has been updated";
			return RedirectToPage("Settings");
		}

		public async Task<IActionResult> OnPostSendVerificationEmail()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);

			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
			await _emailService.EmailConfirmation(user.Email, callbackUrl);

			StatusMessage = "Verification email sent. Please check your email.";
			return RedirectToPage("Settings");
		}
	}
}
