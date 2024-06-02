using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;

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
		if (!db.HasFullTextSearch())
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
			db.ExtendTimeoutForSearch();
			PageResults = await db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrent()
				.WebSearch(SearchTerms)
				.ByWebRanking(SearchTerms)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(w => new PageSearch(EF.Functions.WebSearchToTsQuery(SearchTerms).GetResultHeadline(w.Markup), w.PageName))
				.ToListAsync();

			PostResults = await db.ForumPosts
				.ExcludeRestricted(UserCanSeeRestricted)
				.WebSearch(SearchTerms)
				.ByWebRanking(SearchTerms)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(p => new PostSearch(
					EF.Functions.WebSearchToTsQuery(SearchTerms).GetResultHeadline(p.Text),
					p.Topic!.Title,
					p.Id))
				.ToListAsync();

			GameResults = await db.Games
				.WebSearch(SearchTerms)
				.ByWebRanking(SearchTerms)
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
