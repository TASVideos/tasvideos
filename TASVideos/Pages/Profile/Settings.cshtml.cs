using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.Email;

namespace TASVideos.Pages.Profile;

[Authorize]
public class SettingsModel(UserManager userManager, IEmailService emailService, ApplicationDbContext db) : BasePageModel
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

	public string Username { get; set; } = "";

	[Display(Name = "Current Email")]
	public string Email { get; set; } = "";
	public bool IsEmailConfirmed { get; set; }

	[BindProperty]
	[Display(Name = "Time Zone")]
	public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id;

	[BindProperty]
	[Display(Name = "Allow Movie Ratings to be public?")]
	public bool PublicRatings { get; set; }

	[BindProperty]
	[StringLength(100)]
	public string? Location { get; set; }

	[BindProperty]
	[StringLength(1000)]
	public string? Signature { get; set; }

	[BindProperty]
	[Url]
	[Display(Name = "Avatar URL")]
	public string? Avatar { get; set; }

	[BindProperty]
	[Url]
	[Display(Name = "Mood-variant avatar URL")]
	public string? MoodAvatar { get; set; }

	[BindProperty]
	[Display(Name = "Preferred Pronouns")]
	public PreferredPronounTypes PreferredPronouns { get; set; }

	[BindProperty]
	[Display(Name = "Email On New Private Message?")]
	public bool EmailOnPrivateMessage { get; set; }

	[BindProperty]
	[Display(Name = "Automatically Watch Topics When Posting")]
	public UserPreference AutoWatchTopic { get; set; }

	[BindProperty]
	[Display(Name = "Date Format")]
	public UserDateFormat UserDateFormat { get; set; }

	[BindProperty]
	[Display(Name = "Time Format")]
	public UserTimeFormat UserTimeFormat { get; set; }

	[BindProperty]
	[Display(Name = "Decimal Format")]
	public UserDecimalFormat UserDecimalFormat { get; set; }

	public async Task OnGet()
	{
		var user = await userManager.GetRequiredUser(User);
		Username = user.UserName;
		Email = user.Email;
		IsEmailConfirmed = user.EmailConfirmed;
		TimeZoneId = user.TimeZoneId;
		PublicRatings = user.PublicRatings;
		Location = user.From;
		Signature = user.Signature;
		Avatar = user.Avatar;
		MoodAvatar = user.MoodAvatarUrlBase;
		PreferredPronouns = user.PreferredPronouns;
		EmailOnPrivateMessage = user.EmailOnPrivateMessage;
		AutoWatchTopic = user.AutoWatchTopic ?? UserPreference.Auto;
		UserDateFormat = user.DateFormat;
		UserTimeFormat = user.TimeFormat;
		UserDecimalFormat = user.DecimalFormat;
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var site = userManager.AvatarSiteIsBanned(Avatar);
		if (!string.IsNullOrEmpty(site))
		{
			ModelState.AddModelError($"{nameof(Avatar)}", $"Using {site} to host avatars is not allowed.");
		}

		site = userManager.AvatarSiteIsBanned(MoodAvatar);
		if (!string.IsNullOrEmpty(site))
		{
			ModelState.AddModelError($"{nameof(Avatar)}", $"Using {site} to host avatars is not allowed.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await userManager.GetRequiredUser(User);

		bool hasUserCustomLocaleChanged = user.DateFormat != UserDateFormat || user.TimeFormat != UserTimeFormat || user.DecimalFormat != UserDecimalFormat;

		user.TimeZoneId = TimeZoneId;
		user.PublicRatings = PublicRatings;
		user.From = Location;
		user.Avatar = Avatar;
		user.MoodAvatarUrlBase = User.Has(PermissionTo.UseMoodAvatars) ? MoodAvatar : null;
		user.PreferredPronouns = PreferredPronouns;
		user.EmailOnPrivateMessage = EmailOnPrivateMessage;
		user.AutoWatchTopic = AutoWatchTopic;
		user.DateFormat = UserDateFormat;
		user.TimeFormat = UserTimeFormat;
		user.DecimalFormat = UserDecimalFormat;
		if (User.Has(PermissionTo.EditSignature))
		{
			user.Signature = Signature;
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
