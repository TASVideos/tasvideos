using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Search;

[AllowAnonymous]
public class IndexModel : PageModel
{
	public const int PageSize = 10;
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	[StringLength(100, MinimumLength = 2)]
	[Display(Name = "Search Terms")]
	public string SearchTerms { get; set; } = "";

	[FromQuery]
	public int PageNumber { get; set; } = 1;

	public List<PageSearchModel> PageResults { get; set; } = new();
	public List<PostSearchModel> PostResults { get; set; } = new();
	public List<GameSearchModel> GameResults { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (!_db.Database.IsNpgsql())
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
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			var skip = PageSize * (PageNumber - 1);
			_db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
			PageResults = await _db.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
				.Where(w => w.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.OrderByDescending(w => EF.Functions.ToTsVector(w.Markup).Rank(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(w => new PageSearchModel(EF.Functions.WebSearchToTsQuery(SearchTerms).GetResultHeadline(w.Markup), w.PageName))
				.ToListAsync();

			PostResults = await _db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.Where(p => p.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.OrderByDescending(p => p.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(p => new PostSearchModel(
					EF.Functions.WebSearchToTsQuery(SearchTerms).GetResultHeadline(p.Text),
					p.Topic!.Title,
					p.Id))
				.ToListAsync();

			GameResults = await _db.Games
				.Where(g => EF.Functions.ToTsVector(g.DisplayName + " || " + g.GoodName + " || " + g.Abbreviation + " || " + g.System!.Code).Matches(EF.Functions.WebSearchToTsQuery(SearchTerms)))
				.OrderBy(g => g.DisplayName)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(g => new GameSearchModel(g.Id, g.System!.Code, g.DisplayName))
				.ToListAsync();
		}

		return Page();
	}

	public record PageSearchModel(string Highlight, string PageName);
	public record PostSearchModel(string Highlight, string TopicName, int PostId);
	public record GameSearchModel(int Id, string SystemCode, string DisplayName);
}
