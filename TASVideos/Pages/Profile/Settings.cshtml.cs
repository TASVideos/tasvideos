using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Profile.Models;

namespace TASVideos.Pages.Profile;

[Authorize]
public class SettingsModel(
	UserManager userManager,
	IEmailService emailService,
	ApplicationDbContext db)
	: BasePageModel
{
	public static readonly IEnumerable<SelectListItem> AvailablePronouns = Enum
		.GetValues(typeof(PreferredPronounTypes))
		.Cast<PreferredPronounTypes>()
		.Select(m => new SelectListItem
		{
			Value = ((int)m).ToString(),
			Text = m.EnumDisplayName()
		})
		.ToList();

	public static readonly IEnumerable<SelectListItem> AvailableUserPreferenceTypes = Enum
		.GetValues(typeof(UserPreference))
		.Cast<UserPreference>()
		.Select(m => new SelectListItem
		{
			Value = ((int)m).ToString(),
			Text = m.EnumDisplayName()
		})
		.ToList();

	public static readonly IEnumerable<SelectListItem> AvailableDateFormats = Enum
		.GetValues(typeof(UserDateFormat))
		.Cast<UserDateFormat>()
		.Select(m => new SelectListItem
		{
			Value = ((int)m).ToString(),
			Text = m.EnumDisplayName()
		})
		.ToList();

	public static readonly IEnumerable<SelectListItem> AvailableTimeFormats = Enum
		.GetValues(typeof(UserTimeFormat))
		.Cast<UserTimeFormat>()
		.Select(m => new SelectListItem
		{
			Value = ((int)m).ToString(),
			Text = m.EnumDisplayName()
		})
		.ToList();

	public static readonly IEnumerable<SelectListItem> AvailableDecimalFormats = Enum
		.GetValues(typeof(UserDecimalFormat))
		.Cast<UserDecimalFormat>()
		.Select(m => new SelectListItem
		{
			Value = ((int)m).ToString(),
			Text = m.EnumDisplayName()
		})
		.ToList();

	[BindProperty]
	public ProfileSettingsModel Settings { get; set; } = new();

	public async Task OnGet()
	{
		var user = await userManager.GetRequiredUser(User);
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
			EmailOnPrivateMessage = user.EmailOnPrivateMessage,
			Roles = await userManager.UserRoles(user.Id),
			AutoWatchTopic = user.AutoWatchTopic ?? UserPreference.Auto,
			UserDateFormat = user.DateFormat,
			UserTimeFormat = user.TimeFormat,
			UserDecimalFormat = user.DecimalFormat,
		};
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var bannedSites = userManager.BannedAvatarSites().ToList();
		if (!string.IsNullOrWhiteSpace(Settings.Avatar))
		{
			foreach (var site in bannedSites)
			{
				if (Settings.Avatar.Contains(site))
				{
					ModelState.AddModelError($"{nameof(Settings)}.{nameof(Settings.Avatar)}", $"Using {site} to host avatars is not allowed.");
				}
			}
		}

		if (!string.IsNullOrWhiteSpace(Settings.MoodAvatar))
		{
			foreach (var site in bannedSites)
			{
				if (Settings.MoodAvatar.Contains(site))
				{
					ModelState.AddModelError($"{nameof(Settings)}.{nameof(Settings.MoodAvatar)}", $"Using {site} to host avatars is not allowed.");
				}
			}
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.GetRequiredUser(User);

		bool hasUserCustomLocaleChanged = user.DateFormat != Settings.UserDateFormat || user.TimeFormat != Settings.UserTimeFormat || user.DecimalFormat != Settings.UserDecimalFormat;

		user.TimeZoneId = Settings.TimeZoneId;
		user.PublicRatings = Settings.PublicRatings;
		user.From = Settings.From;
		user.Avatar = Settings.Avatar;
		user.MoodAvatarUrlBase = User.Has(PermissionTo.UseMoodAvatars) ? Settings.MoodAvatar : null;
		user.PreferredPronouns = Settings.PreferredPronouns;
		user.EmailOnPrivateMessage = Settings.EmailOnPrivateMessage;
		user.AutoWatchTopic = Settings.AutoWatchTopic;
		user.DateFormat = Settings.UserDateFormat;
		user.TimeFormat = Settings.UserTimeFormat;
		user.DecimalFormat = Settings.UserDecimalFormat;
		if (User.Has(PermissionTo.EditSignature))
		{
			user.Signature = Settings.Signature;
		}

		await db.SaveChangesAsync();

		if (hasUserCustomLocaleChanged)
		{
			userManager.ClearCustomLocaleCache(User.GetUserId());
		}

		SuccessStatusMessage("Your profile has been updated");
		return BasePageRedirect("Settings");
	}

	public async Task<IActionResult> OnPostSendVerificationEmail()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.GetRequiredUser(User);

		var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
		var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
		await emailService.EmailConfirmation(user.Email, callbackUrl);

		SuccessStatusMessage("Verification email sent. Please check your email.");
		return BasePageRedirect("Settings");
	}
}
