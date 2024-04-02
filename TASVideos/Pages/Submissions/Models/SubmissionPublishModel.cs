using TASVideos.Models;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionPublishModel
{
	[Display(Name = "Select movie to be obsoleted")]
	public int? MovieToObsolete { get; init; }

	[DoNotTrim]
	[Display(Name = "Movie description")]
	public string MovieMarkup { get; init; } = SiteGlobalConstants.DefaultPublicationText;

	[Display(Name = "Movie Filename", Description = "Please follow the convention: xxxv#-yyy where xxx is author name, # is version and yyy is game name. Special characters such as \"&\" and \"/\" and \".\" and spaces must not occur in the filename.")]
	public string MovieFileName { get; init; } = "";

	[Url]
	[Display(Name = "Online-watching URL")]
	[StringLength(500)]
	public string OnlineWatchingUrl { get; init; } = "";

	[StringLength(100)]
	[Display(Name = "Online-watching URL Display Name (Optional)")]
	public string? OnlineWatchUrlName { get; init; }

	[Url]
	[Display(Name = "Mirror site URL")]
	[StringLength(500)]
	public string MirrorSiteUrl { get; init; } = "";

	[Required]
	[Display(Name = "Screenshot", Description = "Your movie packed in a ZIP file (max size: 150k)")]
	public IFormFile? Screenshot { get; init; }

	[Display(Name = "Description", Description = "Caption, describe what happens in the screenshot")]
	public string? ScreenshotDescription { get; init; }

	[DoNotTrim]
	[Display(Name = "Submission description (for quoting, reference, etc)")]
	public string? Markup { get; set; }

	[Display(Name = "System")]
	public string? SystemCode { get; init; }

	[Display(Name = "Region")]
	public string? SystemRegion { get; init; }

	[Display(Name = "Game")]
	public string? Game { get; init; }
	public int GameId { get; init; }

	[Display(Name = "Game Version")]
	public string? GameVersion { get; init; }
	public int VersionId { get; init; }

	[Display(Name = "PublicationClass")]
	public string? PublicationClass { get; init; }
	public string? MovieExtension { get; init; }

	[Display(Name = "Selected Flags")]
	public List<int> SelectedFlags { get; init; } = [];

	[Display(Name = "Selected Tags")]
	public List<int> SelectedTags { get; init; } = [];

	// Not used for edit fields
	public string Title { get; init; } = "";
	public int SystemId { get; init; }
	public int? SystemFrameRateId { get; init; }
	public SubmissionStatus Status { get; init; }
	public int? GameGoalId { get; init; }

	[Display(Name = "Emulator Version")]
	public string? EmulatorVersion { get; init; }
	public string? Branch { get; init; }

	public bool CanPublish => SystemId > 0
		&& SystemFrameRateId.HasValue
		&& GameId > 0
		&& VersionId > 0
		&& GameGoalId > 0
		&& !string.IsNullOrEmpty(PublicationClass)
		&& Status == SubmissionStatus.PublicationUnderway;
}
