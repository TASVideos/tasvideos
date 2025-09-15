using TASVideos.Common;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db, IGameSystemService gameSystemService) : BasePageModel
{
	public static readonly List<SelectListItem> AvailableStatuses = Enum.GetValues<SubmissionStatus>().ToDropDown();
	public IEnumerable<SelectListItem> AvailableYears => Enumerable
		.Range(2000, DateTime.UtcNow.Year + 1 - 2000)
		.OrderByDescending(n => n)
		.ToDropDown();

	// For legacy routes such as Subs-Rej-422up
	[FromRoute]
	public string? Query { get; set; }

	[FromQuery]
	public SubmissionSearchRequest Search { get; set; } = new();

	public PageOf<SubmissionEntry, SubmissionSearchRequest> Submissions { get; set; } = new([], new());

	public List<SelectListItem> SystemList { get; set; } = [];

	public async Task OnGet()
	{
		SystemList = (await gameSystemService.GetAll())
			.ToDropDownList().WithDefaultEntry();

		var search = LegacySubListConverter.ToSearchRequest(Query);
		if (search is not null)
		{
			Search = search;
		}

		// Defaults
		// Note that we do not provide these for GameId, the assumption is that we want to see all submissions of a given game, not just active ones
		if (!Search.Statuses.Any() && string.IsNullOrWhiteSpace(Search.GameId))
		{
			Search.Statuses = !string.IsNullOrWhiteSpace(Search.User) || Search.Years.Any()
				? SubmissionSearchRequest.All
				: SubmissionSearchRequest.Default;
		}

		Submissions = await db.Submissions
			.FilterBy(Search)
			.ToSubListEntry(User.GetUserId())
			.SortedPageOf(Search);
	}

	public class SubmissionEntry : ITimeable, ISubmissionDisplay
	{
		[Sortable]
		public string? System { get; init; }

		[Sortable]
		public string? Game { get; init; }

		[Sortable]
		public string? Goal { get; init; }
		public TimeSpan Time => this.Time();
		public List<string>? By { get; init; }

		[TableIgnore]
		public string? AdditionalAuthors { get; init; }

		[Sortable]
		public DateTime Date { get; init; }

		[Sortable]
		public SubmissionStatus Status { get; init; }

		public VoteCounts? Votes { get; init; }

		[TableIgnore]
		public int Id { get; init; }

		[TableIgnore]
		public int Frames { get; init; }

		[TableIgnore]
		public double FrameRate { get; init; }

		[TableIgnore]
		public string? Judge { get; init; }

		[TableIgnore]
		public string? Publisher { get; init; }

		[TableIgnore]
		public string? IntendedClass { get; init; }

		[TableIgnore]
		public DateTime? SyncedOn { get; set; }
	}

	[PagingDefaults(PageSize = 100, Sort = $"{nameof(SubmissionEntry.Date)}")]
	public class SubmissionSearchRequest : PagingModel, ISubmissionFilter
	{
		public ICollection<int> Years { get; set; } = [];
		public string? System { get; init; }
		public string? User { get; init; }
		public string? GameId { get; set; }
		public int? StartType { get; set; }

		[Display(Name = "Show Sync Verified")]
		public bool? ShowVerified { get; set; }

		public ICollection<SubmissionStatus> Statuses { get; set; } = [];

		public static ICollection<SubmissionStatus> Default =>
		[
			SubmissionStatus.New,
			SubmissionStatus.JudgingUnderWay,
			SubmissionStatus.Accepted,
			SubmissionStatus.PublicationUnderway,
			SubmissionStatus.NeedsMoreInfo,
			SubmissionStatus.Delayed
		];

		public static List<SubmissionStatus> All => [.. Enum.GetValues<SubmissionStatus>()];

		ICollection<string> ISubmissionFilter.Systems => string.IsNullOrWhiteSpace(System)
			? []
			: [System];

		ICollection<int> ISubmissionFilter.GameIds => !string.IsNullOrWhiteSpace(GameId) && int.TryParse(GameId, out int _)
			? [int.Parse(GameId)]
			: [];
	}
}
