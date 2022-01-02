using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Profile.Models;

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

		public static readonly IEnumerable<SelectListItem> AvailablePronouns = Enum
			.GetValues(typeof(PreferredPronounTypes))
			.Cast<PreferredPronounTypes>()
			.Select(m => new SelectListItem
			{
				Value = ((int)m).ToString(),
				Text = m.EnumDisplayName()
			})
			.ToList();

		[BindProperty]
		public ProfileSettingsModel Settings { get; set; } = new ();

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
				From = user.From,
				Signature = user.Signature,
				Avatar = user.Avatar,
				MoodAvatar = user.MoodAvatarUrlBase,
				PreferredPronouns = user.PreferredPronouns,
				Roles = await _db.Users
					.Where(u => u.Id == user.Id)
					.SelectMany(u => u.UserRoles)
					.Select(ur => ur.Role!)
					.Select(r => new RoleDto
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
			if (!string.Equals(Settings.Email, email, StringComparison.CurrentCultureIgnoreCase))
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
			user.MoodAvatarUrlBase = User.Has(PermissionTo.UseMoodAvatars) ? Settings.MoodAvatar : null;
			user.PreferredPronouns = Settings.PreferredPronouns;
			await _db.SaveChangesAsync();

			SuccessStatusMessage("Your profile has been updated");
			return BasePageRedirect("Settings");
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

			SuccessStatusMessage("Verification email sent. Please check your email.");
			return BasePageRedirect("Settings");
		}
	}
}
