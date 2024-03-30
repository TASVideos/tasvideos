using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Helpers;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiLink)]
[TextModule]
public class WikiLink(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string href, string? displayText)
	{
		return View(await InvokeInternal(href, displayText));
	}

	public async Task<string> RenderTextAsync(IWikiPage? pageData, string href, string? displayText)
	{
		WikiLinkModel wikiLinkModel = await InvokeInternal(href, displayText);
		return wikiLinkModel.DisplayText;
	}

	private async Task<WikiLinkModel> InvokeInternal(string href, string? displayText)
	{
		int? id;
		string? titleText = null;

		if (displayText?.StartsWith("user:") == true)
		{
			displayText = displayText[5..];
		}
		else if ((id = SubmissionHelper.IsSubmissionLink(href)).HasValue)
		{
			var title = await GetSubmissionTitle(id.Value);
			if (!string.IsNullOrWhiteSpace(title))
			{
				titleText = title;
			}
		}
		else if ((id = SubmissionHelper.IsPublicationLink(href)).HasValue)
		{
			var title = await GetPublicationTitle(id.Value);
			if (!string.IsNullOrWhiteSpace(title))
			{
				titleText = $"[{id.Value}] " + title;
			}
		}
		else if ((id = SubmissionHelper.IsGamePageLink(href)).HasValue)
		{
			var title = await GetGameTitle(id.Value);
			if (!string.IsNullOrWhiteSpace(title))
			{
				titleText = title;
			}
		}

		if (titleText is not null)
		{
			if (string.IsNullOrWhiteSpace(displayText))
			{
				displayText = titleText;
				titleText = null;
			}
		}

		if (string.IsNullOrWhiteSpace(displayText))
		{
			displayText = href[1..];
		}

		return new WikiLinkModel
		{
			Href = href,
			DisplayText = displayText,
			Title = titleText,
		};
	}

	private async Task<string?> GetPublicationTitle(int id)
	{
		return (await db.Publications
			.Select(s => new { s.Id, s.Title })
			.SingleOrDefaultAsync(s => s.Id == id))?.Title;
	}

	private async Task<string?> GetSubmissionTitle(int id)
	{
		return (await db.Submissions
			.Select(s => new { s.Id, s.Title })
			.SingleOrDefaultAsync(s => s.Id == id))?.Title;
	}

	private async Task<string?> GetGameTitle(int id)
	{
		return (await db.Games
			.Select(g => new { g.Id, g.DisplayName })
			.SingleOrDefaultAsync(g => g.Id == id))?.DisplayName;
	}
}
