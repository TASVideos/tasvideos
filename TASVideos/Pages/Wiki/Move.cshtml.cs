﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.MoveWikiPages)]
public class MoveModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;
	private readonly ExternalMediaPublisher _publisher;

	public MoveModel(
		IWikiPages wikiPages,
		ExternalMediaPublisher publisher)
	{
		_wikiPages = wikiPages;
		_publisher = publisher;
	}

	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public WikiMoveModel Move { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (!string.IsNullOrWhiteSpace(Path))
		{
			Path = Path.Trim('/');
			if (await _wikiPages.Exists(Path))
			{
				Move = new WikiMoveModel
				{
					OriginalPageName = Path,
					DestinationPageName = Path
				};
				return Page();
			}
		}

		return NotFound();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		Move.OriginalPageName = Move.OriginalPageName.Trim('/');
		Move.DestinationPageName = Move.DestinationPageName.Trim('/');

		if (await _wikiPages.Exists(Move.DestinationPageName, includeDeleted: true))
		{
			ModelState.AddModelError("Move.DestinationPageName", "The destination page already exists.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _wikiPages.Move(Move.OriginalPageName, Move.DestinationPageName);

		if (!result)
		{
			ModelState.AddModelError("", "Unable to move page, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await _publisher.SendGeneralWiki(
			$"Page {Move.OriginalPageName} moved to {Move.DestinationPageName} by {User.Name()}",
			"",
			Move.DestinationPageName);

		return BaseRedirect("/" + Move.DestinationPageName);
	}
}
