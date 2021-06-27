﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class DiffModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;

		public DiffModel(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		[FromQuery]
		public string? Path { get; set; }

		[FromQuery]
		public int? FromRevision { get; set; }

		[FromQuery]
		public int? ToRevision { get; set; }

		public WikiDiffModel Diff { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			Path = Path?.Trim('/');

			if (string.IsNullOrWhiteSpace(Path))
			{
				return NotFound();
			}

			Diff = FromRevision.HasValue && ToRevision.HasValue
				? await GetPageDiff(Path, FromRevision.Value, ToRevision.Value)
				: await GetLatestPageDiff(Path);

			return Page();
		}

		public async Task<IActionResult> OnGetDiffData(string path, int fromRevision, int toRevision)
		{
			var data = await GetPageDiff(path.Trim('/'), fromRevision, toRevision);
			return new JsonResult(data);
		}

		private async Task<WikiDiffModel> GetPageDiff(string pageName, int fromRevision, int toRevision)
		{
			var revisions = await _wikiPages.Query
				.ForPage(pageName)
				.Where(wp => wp.Revision == fromRevision
					|| wp.Revision == toRevision)
				.ToListAsync();

			if (revisions.Count != (fromRevision == toRevision ? 1 : 2))
			{
				throw new InvalidOperationException($"Page \"{pageName}\" or revisions {fromRevision}-{toRevision} could not be found");
			}

			return new WikiDiffModel
			{
				PageName = pageName,
				LeftRevision = fromRevision,
				RightRevision = toRevision,
				LeftMarkup = revisions.Single(wp => wp.Revision == fromRevision).Markup,
				RightMarkup = revisions.Single(wp => wp.Revision == toRevision).Markup
			};
		}

		private async Task<WikiDiffModel> GetLatestPageDiff(string pageName)
		{
			var revisions = await _wikiPages.Query
				.ForPage(pageName)
				.ThatAreNotDeleted()
				.OrderByDescending(wp => wp.Revision)
				.Take(2)
				.ToListAsync();

			if (!revisions.Any())
			{
				throw new InvalidOperationException($"Page \"{pageName}\" could not be found");
			}

			// If count is 1, it must be a new page with no history, so compare against nothing
			if (revisions.Count == 1)
			{
				return new WikiDiffModel
				{
					PageName = pageName,
					LeftRevision = revisions.First().Revision - 1,
					LeftMarkup = "",
					RightRevision = revisions.First().Revision,
					RightMarkup = revisions.First().Markup
				};
			}

			return new WikiDiffModel
			{
				PageName = pageName,
				LeftRevision = revisions[1].Revision,
				LeftMarkup = revisions[1].Markup,
				RightRevision = revisions[0].Revision,
				RightMarkup = revisions[0].Markup
			};
		}
	}
}
