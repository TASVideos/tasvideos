using System.Text.RegularExpressions;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Search;

[AllowAnonymous]
public class AdvancedModel(ApplicationDbContext db) : BasePageModel
{
	public const int PageSize = 10;
	public const int PageSizeSingle = 50;

	[FromQuery]
	[StringLength(100, MinimumLength = 2)]
	public string SearchTerms { get; set; } = "";

	[FromQuery]
	[Range(1, int.MaxValue)]
	public int PageNumber { get; set; } = 1;

	public List<PageResult> PageResults { get; set; } = [];
	public List<TopicResult> TopicResults { get; set; } = [];
	public List<PostResult> PostResults { get; set; } = [];
	public List<GameResult> GameResults { get; set; } = [];
	public List<PublicationResult> PublicationResults { get; set; } = [];
	public List<UserResult> UserResults { get; set; } = [];

	[FromQuery]
	public bool SearchWiki { get; set; }

	[FromQuery]
	public bool SearchForumTopics { get; set; }

	[FromQuery]
	public bool SearchForumPosts { get; set; }

	[FromQuery]
	public bool SearchPublications { get; set; } = true;

	[FromQuery]
	public bool SearchGames { get; set; } = true;

	[FromQuery]
	public bool SearchUsers { get; set; }

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
			DisplayPageSize = PageSize;
			if (new[] { SearchWiki, SearchForumTopics, SearchForumPosts, SearchGames }.Count(b => b) == 1)
			{
				DisplayPageSize = PageSizeSingle;
			}

			var skip = DisplayPageSize * (PageNumber - 1);
			db.ExtendTimeoutForSearch();

			if (SearchWiki)
			{
				PageResults = await db.WikiPages
					.ThatAreNotDeleted()
					.ThatAreCurrent()
					.Where(w => Regex.IsMatch(w.PageName, SearchTerms) || Regex.IsMatch(w.Markup, SearchTerms))
					.OrderBy(w => w.PageName)
					.Skip(skip)
					.Take(DisplayPageSize + 1)
					.Select(w => new PageResult(w.Markup.Substring(0, Math.Min(60, w.Markup.Length)), w.PageName))
					.ToListAsync();
			}

			if (SearchForumTopics)
			{
				TopicResults = await db.ForumTopics
				.ExcludeRestricted(UserCanSeeRestricted)
				.Where(t => t.ForumId != SiteGlobalConstants.WorkbenchForumId && t.ForumId != SiteGlobalConstants.PlaygroundForumId && t.ForumId != SiteGlobalConstants.PublishedMoviesForumId && t.ForumId != SiteGlobalConstants.GrueFoodForumId)
				.Where(t => Regex.IsMatch(t.Title, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(t => t.CreateTimestamp)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(t => new TopicResult(
					t.Title,
					t.Id,
					t.Forum!.Name))
				.ToListAsync();
			}

			if (SearchForumPosts)
			{
				PostResults = await db.ForumPosts
				.ExcludeRestricted(UserCanSeeRestricted)
				.Where(p => Regex.IsMatch(p.Text, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(p => p.CreateTimestamp)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(p => new PostResult(
					p.Text,
					Regex.Match(p.Text, "(^|[^A-Za-z])" + SearchTerms, RegexOptions.IgnoreCase).Index,
					p.Topic!.Title,
					p.Id))
				.ToListAsync();
			}

			if (SearchGames)
			{
				GameResults = await db.Games
				.Where(g => Regex.IsMatch(g.DisplayName, "(^|[^A-Za-z])" + SearchTerms))
				.OrderByDescending(g => g.Publications.Count)
				.ThenByDescending(g => g.Submissions.Count)
				.ThenBy(g => g.DisplayName)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(g => new GameResult(g.Id, g.DisplayName))
				.ToListAsync();
			}

			if (SearchPublications)
			{
				PublicationResults = await db.Publications
				.Where(p => Regex.IsMatch(p.Title, "(^|[^A-Za-z])" + SearchTerms))
				.OrderBy(p => p.Title)
				.Skip(skip)
				.Take(DisplayPageSize + 1)
				.Select(p => new PublicationResult(p.Id, p.Title))
				.ToListAsync();
			}

			if (SearchUsers)
			{
				UserResults = await db.Users
					.Where(u => Regex.IsMatch(u.UserName, "(^|[^A-Za-z])" + SearchTerms))
					.OrderBy(u => u.UserName)
					.Skip(skip)
					.Take(DisplayPageSize + 1)
					.Select(u => new UserResult(u.Id, u.UserName, u.UserRoles.Select(ur => ur.Role!.Name)))
					.ToListAsync();
			}

			EnablePrev = PageNumber > 1;
			EnableNext = new[] { PageResults.Count, TopicResults.Count, PostResults.Count, GameResults.Count, PublicationResults.Count }.Any(c => c > DisplayPageSize);
		}

		return Page();
	}

	public record PageResult(string Text, string PageName);
	public record TopicResult(string TopicName, int TopicId, string SubforumName);
	public record PostResult(string Text, int Index, string TopicName, int PostId);
	public record GameResult(int Id, string DisplayName);
	public record PublicationResult(int Id, string Title);
	public record UserResult(int Id, string UserName, IEnumerable<string> Roles);
}
