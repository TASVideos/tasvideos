using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Search;

[AllowAnonymous]
public class AdvancedModel(ApplicationDbContext db) : BasePageModel
{
	public const int PageSize = 10;
	public const int PageSizeSingle = 50;

	[FromQuery]
	[StringLength(100, MinimumLength = 2)]
	[Display(Name = "Search Terms")]
	public string SearchTerms { get; set; } = "";

	[FromQuery]
	public int PageNumber { get; set; } = 1;

	public List<PageSearchModel> PageResults { get; set; } = [];
	public List<TopicSearchModel> TopicResults { get; set; } = [];
	public List<PostSearchModel> PostResults { get; set; } = [];
	public List<GameSearchModel> GameResults { get; set; } = [];
	public List<PublicationSearchModel> PublicationResults { get; set; } = [];

	[FromQuery]
	[Display(Name = "Search Wiki")]
	public bool PageSearch { get; set; } = false;

	[FromQuery]
	[Display(Name = "Search Forum Topics")]
	public bool TopicSearch { get; set; } = false;

	[FromQuery]
	[Display(Name = "Search Forum Posts")]
	public bool PostSearch { get; set; } = false;

	[FromQuery]
	[Display(Name = "Search Publications")]
	public bool PublicationSearch { get; set; } = true;

	[FromQuery]
	[Display(Name = "Search Games")]
	public bool GameSearch { get; set; } = true;

	public int DisplayPageSize { get; set; } = PageSize;
	public bool EnablePrev { get; set; }
	public bool EnableNext { get; set; }

	public async Task<IActionResult> OnGet()
	{
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
			DisplayPageSize = PageSize;
			if (new[] { PageSearch, TopicSearch, PostSearch, GameSearch }.Count(b => b) == 1)
			{
				DisplayPageSize = PageSizeSingle;
			}

			var skip = DisplayPageSize * (PageNumber - 1);
			db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));

			if (PageSearch)
			{
				PageResults = await db.WikiPages
					.ThatAreNotDeleted()
					.ThatAreCurrent()
					.Where(w => Regex.IsMatch(w.PageName, SearchTerms) || Regex.IsMatch(w.Markup, SearchTerms))
					.OrderBy(w => w.PageName)
					.Skip(skip)
					.Take(DisplayPageSize + 1)
					.Select(w => new PageSearchModel(w.Markup.Substring(0, Math.Min(60, w.Markup.Length)), w.PageName))
					.ToListAsync();
			}

			if (TopicSearch)
			{
				TopicResults = await db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.ForumId != SiteGlobalConstants.WorkbenchForumId && t.ForumId != SiteGlobalConstants.PlaygroundForumId && t.ForumId != SiteGlobalConstants.PublishedMoviesForumId && t.ForumId != SiteGlobalConstants.GrueFoodForumId)
				.Where(t => Regex.IsMatch(t.Title, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(t => t.CreateTimestamp)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(t => new TopicSearchModel(
					t.Title,
					t.Id,
					t.Forum!.Name))
				.ToListAsync();
			}

			if (PostSearch)
			{
				PostResults = await db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.Where(p => Regex.IsMatch(p.Text, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(p => p.CreateTimestamp)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(p => new PostSearchModel(
					p.Text,
					Regex.Match(p.Text, "(^|[^A-Za-z])" + SearchTerms, RegexOptions.IgnoreCase).Index,
					p.Topic!.Title,
					p.Id))
				.ToListAsync();
			}

			if (GameSearch)
			{
				GameResults = await db.Games
				.Where(g => Regex.IsMatch(g.DisplayName, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(g => g.Publications.Count)
				.ThenByDescending(g => g.Submissions.Count)
				.ThenBy(g => g.DisplayName)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(g => new GameSearchModel(g.Id, g.DisplayName))
				.ToListAsync();
			}

			if (PublicationSearch)
			{
				PublicationResults = await db.Publications
				.Where(p => Regex.IsMatch(p.Title, "(^|[^A-Za-z])" + SearchTerms))
				.OrderBy(p => p.Title)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(p => new PublicationSearchModel(p.Id, p.Title))
				.ToListAsync();
			}

			EnablePrev = PageNumber > 1;
			EnableNext = new[] { PageResults.Count, TopicResults.Count, PostResults.Count, GameResults.Count, PublicationResults.Count }.Any(c => c > DisplayPageSize);
		}

		return Page();
	}

	public record PageSearchModel(string Text, string PageName);
	public record TopicSearchModel(string TopicName, int TopicId, string SubforumName);
	public record PostSearchModel(string Text, int Index, string TopicName, int PostId);
	public record GameSearchModel(int Id, string DisplayName);
	public record PublicationSearchModel(int Id, string Title);
}
