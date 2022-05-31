﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[RequireEdit]
public class EditModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;

	public EditModel(
		IWikiPages wikiPages,
		ApplicationDbContext db,
		ExternalMediaPublisher publisher)
	{
		_wikiPages = wikiPages;
		_db = db;
		_publisher = publisher;
	}

	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public WikiEditModel PageToEdit { get; set; } = new();

	public int? Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Path = Path?.Trim('/');
		if (string.IsNullOrWhiteSpace(Path))
		{
			return NotFound();
		}

		if (!WikiHelper.IsValidWikiPageName(Path))
		{
			return NotFound();
		}

		if (WikiHelper.IsHomePage(Path) && !await UserNameExists(Path))
		{
			return NotFound();
		}

		var page = await _wikiPages.Page(Path);

		PageToEdit = new WikiEditModel
		{
			Markup = page?.Markup ?? ""
		};
		Id = page?.Id;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		Path = Path?.Trim('/');
		if (string.IsNullOrWhiteSpace(Path))
		{
			return NotFound();
		}

		if (!WikiHelper.IsValidWikiPageName(Path))
		{
			return Home();
		}

		if (WikiHelper.IsHomePage(Path) && !await UserNameExists(Path))
		{
			return Home();
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var page = new WikiPage
		{
			CreateTimestamp = PageToEdit.EditStart,
			PageName = Path.Trim('/'),
			Markup = PageToEdit.Markup,
			MinorEdit = PageToEdit.MinorEdit,
			RevisionMessage = PageToEdit.RevisionMessage,
			AuthorId = User.GetUserId()
		};
		var result = await _wikiPages.Add(page);
		if (!result)
		{
			ModelState.AddModelError("", "Unable to save. The content on this page may have been modified by another user.");
			return Page();
		}

		var subId = WikiHelper.IsSubmissionPage(page.PageName);
		if (subId.HasValue)
		{
			var sub = await _db.Submissions.SingleOrDefaultAsync(s => s.Id == subId.Value);
			if (sub != null)
			{
				sub.WikiContentId = page.Id;
				await _db.SaveChangesAsync();
			}
		}

		var pubId = WikiHelper.IsPublicationPage(page.PageName);
		if (pubId.HasValue)
		{
			var pub = await _db.Publications.SingleOrDefaultAsync(p => p.Id == pubId.Value);
			if (pub != null)
			{
				pub.WikiContentId = page.Id;
				await _db.SaveChangesAsync();
			}
		}

		if (page.Revision == 1 || !PageToEdit.MinorEdit)
		{
			await Announce(page);
		}

		return BaseRedirect("/" + page.PageName);
	}

	public async Task<IActionResult> OnPostRollbackLatest()
	{
		var latestRevision = await _wikiPages.Page(Path);
		if (latestRevision is null)
		{
			return NotFound();
		}

		if (latestRevision.Revision == 1)
		{
			return BadRequest("Cannot rollback the first revision of a page, just delete instead.");
		}

		var previousRevision = await _wikiPages.Query
			.Where(wp => wp.PageName == Path)
			.ThatAreNotCurrent()
			.OrderByDescending(wp => wp.Revision)
			.FirstOrDefaultAsync();

		if (previousRevision is null)
		{
			return NotFound();
		}

		var rollBackRevision = new WikiPage
		{
			PageName = Path!,
			RevisionMessage = $"Rolling back Revision {latestRevision.Revision} \"{latestRevision.RevisionMessage}\"",
			Markup = previousRevision.Markup,
			AuthorId = User.GetUserId(),
			MinorEdit = false
		};

		await _wikiPages.Add(rollBackRevision);
		await Announce(rollBackRevision);

		return BasePageRedirect("PageHistory", new { Path, Latest = true });
	}

	private async Task<bool> UserNameExists(string path)
	{
		var userName = WikiHelper.ToUserName(path);
		return await _db.Users.Exists(userName);
	}

	private async Task Announce(WikiPage page)
	{
		await _publisher.SendGeneralWiki(
			$"Page {Path} {(page.Revision > 1 ? "edited" : "created")} by {User.Name()}",
			$"{page.RevisionMessage}",
			Path!);
	}
}
