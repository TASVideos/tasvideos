﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
	public class DeletedPagesModel : BasePageModel
	{
		private readonly ExternalMediaPublisher _publisher;
		private readonly IWikiPages _wikiPages;

		public DeletedPagesModel(
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages)
		{
			_publisher = publisher;
			_wikiPages = wikiPages;
		}

		public IEnumerable<DeletedWikiPageDisplayModel> DeletedPages { get; set; } = new List<DeletedWikiPageDisplayModel>();

		public async Task OnGet()
		{
			DeletedPages = await _wikiPages.Query
				.ThatAreDeleted()
				.GroupBy(tkey => tkey.PageName)
				.Select(record => new DeletedWikiPageDisplayModel
				{
					PageName = record.Key,
					RevisionCount = record.Count()

					// https://github.com/aspnet/EntityFrameworkCore/issues/3103
					// EF Core 2.1 bug, this no longer works, "Must be reducible node exception
					// HasExistingRevisions = _wikiPages.Query.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)
				})
				.ToListAsync();

			// Workaround for EF Core 2.1 issue
			// https://github.com/aspnet/EntityFrameworkCore/issues/3103
			// We can use the cache to potentially avoid n+1 trips to the db
			foreach (var result in DeletedPages)
			{
				result.HasExistingRevisions = await _wikiPages.Exists(result.PageName);
			}
		}

		public async Task<IActionResult> OnPostDeletePage(string path)
		{
			if (!User.Has(PermissionTo.DeleteWikiPages))
			{
				return AccessDenied();
			}

			if (!string.IsNullOrWhiteSpace(path))
			{
				var result = await _wikiPages.Delete(path.Trim('/'));

				if (result == -1)
				{
					ModelState.AddModelError("", "Unable to delete page, the page may have been modified during the saving of this operation.");
					return Page();
				}

				_publisher.SendGeneralWiki(
					$"Page {path} DELETED by {User.Name()}",
					$"({result} revisions)",
					"",
					User.Name());
			}

			return RedirectToPage("DeletedPages");
		}

		public async Task<IActionResult> OnPostDeleteRevision(string path, int revision)
		{
			if (!User.Has(PermissionTo.DeleteWikiPages))
			{
				return AccessDenied();
			}

			if (string.IsNullOrWhiteSpace(path) || revision == 0)
			{
				return Home();
			}

			path = path.Trim('/');
			await _wikiPages.Delete(path, revision);

			_publisher.SendGeneralWiki(
					$"Revision {revision} of Page {path} DELETED by {User.Name()}",
					"",
					"",
					User.Name());

			return Redirect("/" + path);
		}

		public async Task<IActionResult> OnPostUndelete(string path)
		{
			if (!User.Has(PermissionTo.DeleteWikiPages))
			{
				return AccessDenied();
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				return Home();
			}

			path = path.Trim('/');
			var result = await _wikiPages.Undelete(path);
			if (!result)
			{
				ModelState.AddModelError("", "Unable to undelete, the page may have been modified during the saving of this operation.");
				return Page();
			}

			_publisher.SendGeneralWiki(
					$"Page {path} UNDELETED by {User.Name()}",
					"",
					path,
					User.Name());

			return Redirect("/" + path);
		}
	}
}
