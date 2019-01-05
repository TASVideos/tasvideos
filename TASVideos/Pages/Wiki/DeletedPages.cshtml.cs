using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
	public class DeletedPagesModel : BasePageModel
	{
		private readonly ExternalMediaPublisher _publisher;
		private readonly IWikiPages _wikiPages;
		private readonly WikiTasks _wikiTasks;

		public DeletedPagesModel(
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			WikiTasks wikiTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_publisher = publisher;
			_wikiPages = wikiPages;
			_wikiTasks = wikiTasks;
		}

		public IEnumerable<DeletedWikiPageDisplayModel> DeletedPages { get; set; } = new List<DeletedWikiPageDisplayModel>();

		public async Task OnGet()
		{
			DeletedPages = await _wikiTasks.GetDeletedPages();
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
