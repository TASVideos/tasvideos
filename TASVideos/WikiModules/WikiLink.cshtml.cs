using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Settings;
using TASVideos.Data.Helpers;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiLink)]
[TextModule]
[MetaDescriptionModule]
public class WikiLink(ApplicationDbContext db, AppSettings settings) : WikiViewComponent
{
	public string Href { get; set; } = "";
	public string DisplayText { get; set; } = "";
	public string? Title { get; set; }

	public async Task<IViewComponentResult> InvokeAsync(string href, string? displayText, string? implicitDisplayText)
	{
		await GenerateLink(href, displayText, implicitDisplayText);
		return View();
	}

	public async Task<string> RenderTextAsync(IWikiPage? pageData, string href, string? displayText, string? implicitDisplayText)
	{
		await GenerateLink(href, displayText, implicitDisplayText);
		return $"{DisplayText} ( {AbsoluteUrl(Href)} )";
	}

	public async Task<string> RenderMetaDescriptionAsync(IWikiPage? pageData, string href, string? displayText, string? implicitDisplayText)
	{
		await GenerateLink(href, displayText, implicitDisplayText);
		return DisplayText;
	}

	private string AbsoluteUrl(string url)
	{
		if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsed))
		{
			return url;
		}

		if (!parsed.IsAbsoluteUri)
		{
			return $"{settings.BaseUrl}/{url.TrimStart('/')}";
		}

		return url;
	}

	private async Task GenerateLink(string href, string? displayText, string? implicitDisplayText)
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
			displayText = implicitDisplayText ?? href[1..];
		}

		Href = href;
		DisplayText = displayText;
		Title = titleText;
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
