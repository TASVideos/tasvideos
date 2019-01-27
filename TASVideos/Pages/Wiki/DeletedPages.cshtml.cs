using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
	public class DeletedPagesModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IWikiPages _wikiPages;

		public DeletedPagesModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			UserManager userManager) 
			: base(userManager)
		{
			_db = db;
			_publisher = publisher;
			_wikiPages = wikiPages;
		}

		public IEnumerable<DeletedWikiPageDisplayModel> DeletedPages { get; set; } = new List<DeletedWikiPageDisplayModel>();

		public async Task OnGet()
		{
			DeletedPages = await _db.WikiPages
				.ThatAreDeleted()
				.GroupBy(tkey => tkey.PageName)
				.Select(record => new DeletedWikiPageDisplayModel
				{
					PageName = record.Key,
					RevisionCount = record.Count(),

					// https://github.com/aspnet/EntityFrameworkCore/issues/3103
					// EF Core 2.1 bug, this no longer works, "Must be reducible node exception
					// HasExistingRevisions = _db.WikiPages.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)
				})
				.ToListAsync();

			// Workaround for EF Core 2.1 issue
			// https://github.com/aspnet/EntityFrameworkCore/issues/3103
			// Since we know the cache is up to date we can do the logic there and avoid n+1 trips to the db
			await _wikiPages.PreLoadCache();
			foreach (var result in DeletedPages)
			{
				result.HasExistingRevisions = _wikiPages.Any(wp => wp.PageName == result.PageName);
			}
		}

		public async Task<IActionResult> OnGetDeletePage(string path)
		{
			if (!UserHas(PermissionTo.DeleteWikiPages))
			{
				return AccessDenied();
			}

			if (!string.IsNullOrWhiteSpace(path))
			{
				var result = await _wikiPages.Delete(path.Trim('/'));

				_publisher.SendGeneralWiki(
					$"Page {path} DELETED by {User.Identity.Name}",
					$"({result} revisions)",
					"");
			}

			return RedirectToPage("DeletedPages");
		}

		public async Task<IActionResult> OnGetDeleteRevision(string path, int revision)
		{
			if (!UserHas(PermissionTo.DeleteWikiPages))
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
					$"Revision {revision} of Page {path} DELETED by {User.Identity.Name}",
					"",
					"");

			return Redirect("/" + path);
		}

		public async Task<IActionResult> OnGetUndelete(string path)
		{
			if (!UserHas(PermissionTo.DeleteWikiPages))
			{
				return AccessDenied();
			}
			
			if (string.IsNullOrWhiteSpace(path))
			{
				return Home();
			}

			path = path.Trim('/');
			await _wikiPages.Undelete(path);

			_publisher.SendGeneralWiki(
					$"Page {path} UNDELETED by {User.Identity.Name}",
					"",
					$"{BaseUrl}/path");

			return Redirect("/" + path);
		}
	}
}
