using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Search;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public const int PageSize = 10;

	[FromQuery]
	[StringLength(100, MinimumLength = 2)]
	[Display(Name = "Search Terms")]
	public string SearchTerms { get; set; } = "";

	[FromQuery]
	public int PageNumber { get; set; } = 1;

	public List<PageSearch> PageResults { get; set; } = [];
	public List<PostSearch> PostResults { get; set; } = [];
	public List<GameSearch> GameResults { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		if (!db.Database.IsNpgsql())
		{
			ModelState.AddModelError("", "This feature is not currently available.");
			return BadRequest(ModelState);
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!string.IsNullOrWhiteSpace(SearchTerms))
		{
			var skip = PageSize * (PageNumber - 1);
			db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
			PageResults = await db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrent()
				.Where(w => w.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.OrderByDescending(w => EF.Functions.ToTsVector(w.Markup).Rank(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(w => new PageSearch(EF.Functions.WebSearchToTsQuery(SearchTerms).GetResultHeadline(w.Markup), w.PageName))
				.ToListAsync();

			PostResults = await db.ForumPosts
				.ExcludeRestricted(UserCanSeeRestricted)
				.Where(p => p.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.OrderByDescending(p => p.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(p => new PostSearch(
					EF.Functions.WebSearchToTsQuery(SearchTerms).GetResultHeadline(p.Text),
					p.Topic!.Title,
					p.Id))
				.ToListAsync();

			GameResults = await db.Games
				.Where(g => EF.Functions.ToTsVector("simple", g.DisplayName.Replace("/", " ") + " || " + g.Aliases + " || " + g.Abbreviation).Matches(EF.Functions.WebSearchToTsQuery("simple", SearchTerms)))
				.OrderByDescending(g => EF.Functions.ToTsVector("simple", g.DisplayName.Replace("/", " ")).ToStripped().Rank(EF.Functions.WebSearchToTsQuery("simple", SearchTerms), NpgsqlTsRankingNormalization.DivideByLength))
					.ThenBy(g => g.DisplayName.Length)
					.ThenBy(g => g.DisplayName)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(g => new GameSearch(
					g.Id,
					g.DisplayName,
					g.GameVersions.Select(v => v.System!.Code),
					g.GameGroups.Select(gg => new GameGroupEntry(gg.GameGroupId, gg.GameGroup!.Name))))
				.ToListAsync();
		}

		return Page();
	}

	public record PageSearch(string Highlight, string PageName);
	public record PostSearch(string Highlight, string TopicName, int PostId);
	public record GameSearch(int Id, string DisplayName, IEnumerable<string> Systems, IEnumerable<GameGroupEntry> Groups);
	public record GameGroupEntry(int Id, string Name);
}
