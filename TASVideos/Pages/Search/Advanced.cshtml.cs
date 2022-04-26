using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Search;

[AllowAnonymous]
public class AdvancedModel : PageModel
{
	public const int PageSize = 10;
	private readonly ApplicationDbContext _db;

	public AdvancedModel(ApplicationDbContext db)
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

		// there is no dedicated method to check whether a regex string is valid, so we use try catch
		try
		{
			Regex.Match("", SearchTerms);
		}
		catch (ArgumentException)
		{
			ModelState.AddModelError("SearchTerms", "Invalid Regular Expression.");
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
				.Where(w => Regex.IsMatch(w.PageName, SearchTerms))
				.OrderBy(w => w.PageName)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(w => new PageSearchModel(w.Markup.Substring(0, Math.Min(60, w.Markup.Length)), w.PageName))
				.ToListAsync();

			PostResults = await _db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.Where(p => Regex.IsMatch(p.Text, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(p => p.CreateTimestamp)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(p => new PostSearchModel(
					p.Text,
					Regex.Match(p.Text, "(^|[^A-Za-z])" + SearchTerms, RegexOptions.IgnoreCase).Index,
					p.Topic!.Title,
					p.Id))
				.ToListAsync();

			GameResults = await _db.Games
				.Where(g => Regex.IsMatch(g.DisplayName, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(g => g.Publications.Count)
				.ThenByDescending(g => g.Submissions.Count)
				.ThenBy(g => g.DisplayName)
				.Skip(skip)
				.Take(PageSize + 1)
				.Select(g => new GameSearchModel(g.Id, g.System!.Code, g.DisplayName))
				.ToListAsync();
		}

		return Page();
	}

	public record PageSearchModel(string Text, string PageName);
	public record PostSearchModel(string Text, int Index, string TopicName, int PostId);
	public record GameSearchModel(int Id, string SystemCode, string DisplayName);
}
