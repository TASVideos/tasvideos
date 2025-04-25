namespace TASVideos.Pages.Profile;

[RequirePermission(PermissionTo.RateMovies)]
public class UnratedModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public UnratedRequest Search { get; set; } = new();

	public PageOf<UnratedMovie, UnratedRequest> UnratedMovies { get; set; } = new([], new());

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		UnratedMovies = await db.Publications
			.ThatAreCurrent()
			.Where(p => p.PublicationRatings.All(pr => pr.UserId != userId))
			.Select(p => new UnratedMovie { Id = p.Id, Title = p.Title, SystemCode = p.System!.Code, Date = p.CreateTimestamp })
			.SortedPageOf(Search);
	}

	public class UnratedMovie
	{
		public int Id { get; init; }

		[Sortable]
		public string SystemCode { get; init; } = "";

		[Sortable]
		public string Title { get; init; } = "";

		[Sortable]
		public DateTime Date { get; init; }
	}

	[PagingDefaults(PageSize = 50, Sort = "Date")]
	public class UnratedRequest : PagingModel;
}
