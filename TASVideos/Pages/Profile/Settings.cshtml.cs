using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class SettingsModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly IEmailSender _emailSender;
		private readonly ApplicationDbContext _db;

		public SettingsModel(
			UserManager userManager,
			IEmailSender emailSender,
			ApplicationDbContext db) 
			: base(userManager)
		{
			_userManager = userManager;
			_emailSender = emailSender;
			_db = db;
		}

		[TempData]
		public string StatusMessage { get; set; }

		// TODO: rename this model, this is not the index page
		[BindProperty]
		public ProfileIndexModel Settings { get; set; } = new ProfileIndexModel();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Settings = new ProfileIndexModel
			{
				Username = user.UserName,
				Email = user.Email,
				TimeZoneId = user.TimeZoneId,
				IsEmailConfirmed = user.EmailConfirmed,
				PublicRatings = user.PublicRatings,
				StatusMessage = StatusMessage,
				From = user.From,
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
			var email = user.Email;
			await _emailSender.SendEmailConfirmationAsync(email, callbackUrl);

			StatusMessage = "Verification email sent. Please check your email.";
			return RedirectToPage("Settings");
		}
	}
}
