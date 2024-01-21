using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Helpers;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.WikiLink)]
public class WikiLink : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public WikiLink(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(string href, string? displayText)
	{
		var model = new WikiLinkModel
		{
			Href = href,
			DisplayText = string.IsNullOrWhiteSpace(displayText)
				? href[1..] // almost always want to chop off the leading '/'
				: displayText
		};

		int? id;

		if (model.DisplayText.StartsWith("user:"))
		{
			model.DisplayText = model.DisplayText[5..];
		}
		else if ((id = SubmissionHelper.IsSubmissionLink(href)).HasValue)
		{
			var title = await GetSubmissionTitle(id.Value);
			if (!string.IsNullOrWhiteSpace(title))
			{
				model.DisplayText = title;
			}
		}
		else if ((id = SubmissionHelper.IsPublicationLink(href)).HasValue)
		{
			var title = await GetPublicationTitle(id.Value);
			if (!string.IsNullOrWhiteSpace(title))
			{
				model.DisplayText = $"[{id.Value}] " + title;
			}
		}
		else if ((id = SubmissionHelper.IsGamePageLink(href)).HasValue)
		{
			var title = await GetGameTitle(id.Value);
			if (!string.IsNullOrWhiteSpace(title))
			{
				model.DisplayText = title;
			}
		}

		return View(model);
	}

	private async Task<string?> GetPublicationTitle(int id)
	{
		return (await _db.Publications
			.Select(s => new { s.Id, s.Title })
			.SingleOrDefaultAsync(s => s.Id == id))?.Title;
	}

	private async Task<string?> GetSubmissionTitle(int id)
	{
		return (await _db.Submissions
			.Select(s => new { s.Id, s.Title })
			.SingleOrDefaultAsync(s => s.Id == id))?.Title;
	}

	private async Task<string?> GetGameTitle(int id)
	{
		return (await _db.Games
			.Select(s => new { s.Id, s.DisplayName })
			.SingleOrDefaultAsync(s => s.Id == id))?.DisplayName;
	}
}
