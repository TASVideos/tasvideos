using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Profile;

[Authorize]
public class SettingsModel(
	UserManager userManager,
	IEmailService emailService,
	ApplicationDbContext db)
	: BasePageModel
{
	public static readonly List<SelectListItem> AvailablePronouns = Enum
		.GetValues<PreferredPronounTypes>()
		.ToDropDown();

	public static readonly List<SelectListItem> AvailableUserPreferenceTypes = Enum
		.GetValues<UserPreference>()
		.ToDropDown();

	public static readonly List<SelectListItem> AvailableDateFormats = Enum
		.GetValues<UserDateFormat>()
		.ToDropDown();

	public static readonly List<SelectListItem> AvailableTimeFormats = Enum
		.GetValues<UserTimeFormat>()
		.ToDropDown();

	public static readonly List<SelectListItem> AvailableDecimalFormats = Enum
		.GetValues<UserDecimalFormat>()
		.ToDropDown();

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

	public class ProfileSettingsModel
	{
		public string Username { get; init; } = "";

		public bool IsEmailConfirmed { get; init; }

		[Display(Name = "Current Email")]
		public string Email { get; init; } = "";

		[Display(Name = "Time Zone")]
		public string TimeZoneId { get; init; } = TimeZoneInfo.Utc.Id;

		[Display(Name = "Allow Movie Ratings to be public?")]
		public bool PublicRatings { get; init; }

		[Display(Name = "Location")]
		public string? From { get; init; }

		[StringLength(1000)]
		public string? Signature { get; init; }

		[Url]
		[Display(Name = "Avatar URL")]
		public string? Avatar { get; init; }

		[Url]
		[Display(Name = "Mood-variant avatar URL")]
		public string? MoodAvatar { get; init; }

		[Display(Name = "Preferred Pronouns")]
		public PreferredPronounTypes PreferredPronouns { get; init; }

		[Display(Name = "Email On New Private Message?")]
		public bool EmailOnPrivateMessage { get; init; }

		[Display(Name = "Automatically Watch Topics When Posting")]
		public UserPreference AutoWatchTopic { get; init; }

		[Display(Name = "Date Format")]
		public UserDateFormat UserDateFormat { get; init; }

		[Display(Name = "Time Format")]
		public UserTimeFormat UserTimeFormat { get; init; }

		[Display(Name = "Decimal Format")]
		public UserDecimalFormat UserDecimalFormat { get; init; }

		public List<RoleDto> Roles { get; init; } = [];
	}
}
