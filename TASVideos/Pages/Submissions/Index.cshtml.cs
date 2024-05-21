using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Common;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	private static readonly List<SelectListItem> Statuses = Enum.GetValues<SubmissionStatus>().ToDropDown();

	// For legacy routes such as Subs-Rej-422up
	[FromRoute]
	public string? Query { get; set; }

	[FromQuery]
	public SubmissionSearchRequest Search { get; set; } = new();

	public SubmissionPageOf<SubmissionEntry> Submissions { get; set; } = new([]);

	public List<SelectListItem> AvailableStatuses => Statuses;

	public List<SelectListItem> SystemList { get; set; } = [];

	public async Task OnGet()
	{
		SystemList = (await db.GameSystems
			.ToDropDownList())
			.WithDefaultEntry();

		var search = LegacySubListConverter.ToSearchRequest(Query);
		if (search is not null)
		{
			Search = search;
		}

		// Defaults
		// Note that we do not provide these for GameId, the assumption is that we want to see all submissions of a given game, not just active ones
		if (!Search.StatusFilter.Any() && string.IsNullOrWhiteSpace(Search.GameId))
		{
			Search.StatusFilter = !string.IsNullOrWhiteSpace(Search.User) || Search.Years.Any()
				? SubmissionSearchRequest.All
				: SubmissionSearchRequest.Default;
		}

		var entries = await db.Submissions
			.FilterBy(Search)
			.ToSubListEntry()
			.SortedPageOf(Search);

		Submissions = new SubmissionPageOf<SubmissionEntry>(entries)
		{
			Years = Search.Years,
			StatusFilter = Search.StatusFilter,
			System = Search.System,
			GameId = Search.GameId,
			User = Search.User
		};
	}

	public class SubmissionEntry : ITimeable, ISubmissionDisplay
	{
		[Sortable]
		public string? System { get; init; }

		[Sortable]
		[Display(Name = "Game")]
		public string? GameName { get; init; }

		[Sortable]
		public string? Branch { get; init; }
		public TimeSpan Time => this.Time();

		[Display(Name = "By")]
		public List<string>? Authors { get; init; }
		[TableIgnore]
		public string? AdditionalAuthors { get; init; }

		[Sortable]
		[Display(Name = "Date")]
		public DateTime Submitted { get; init; }

		[Sortable]
		public SubmissionStatus Status { get; init; }

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
	}

	public class SubmissionPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
	{
		public IEnumerable<int> Years { get; set; } = [];
		public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = [];
		public string? System { get; set; }
		public string? User { get; set; }
		public string? GameId { get; set; }
	}

	public class SubmissionSearchRequest : PagingModel, ISubmissionFilter
	{
		public SubmissionSearchRequest()
		{
			Sort = $"{nameof(SubmissionEntry.Submitted)}";
			PageSize = 100;
		}

		public ICollection<int> Years { get; set; } = [];

		public List<int> AvailableYears => [.. Enumerable
			.Range(2000, DateTime.UtcNow.Year + 1 - 2000)
			.OrderByDescending(n => n)];

		public string? System { get; init; }
		public string? User { get; init; }
		public string? GameId { get; set; }
		public int? StartType { get; set; }

		[Display(Name = "Statuses")]
		public ICollection<SubmissionStatus> StatusFilter { get; set; } = [];

		public static ICollection<SubmissionStatus> Default =>
		[
			SubmissionStatus.New,
			SubmissionStatus.JudgingUnderWay,
			SubmissionStatus.Accepted,
			SubmissionStatus.PublicationUnderway,
			SubmissionStatus.NeedsMoreInfo,
			SubmissionStatus.Delayed
		];

		public static List<SubmissionStatus> All => Enum
			.GetValues<SubmissionStatus>()
			.ToList();

		ICollection<string> ISubmissionFilter.Systems => string.IsNullOrWhiteSpace(System)
			? []
			: [System];

		ICollection<int> ISubmissionFilter.GameIds => !string.IsNullOrWhiteSpace(GameId) && int.TryParse(GameId, out int _)
			? [int.Parse(GameId)]
			: [];
	}
}
