using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Wiki.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Wiki
{
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
		public string Path { get; set; }

		[BindProperty]
		public WikiEditModel PageToEdit { get; set; }

		public int? Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Path = Path?.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(Path))
			{
				return Home();
			}

			if (WikiHelper.IsHomePage(Path) && !await UserNameExists(Path))
			{
				return Home();
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
				PageName = Path.Trim('/'),
				Markup = PageToEdit.Markup,
				MinorEdit = PageToEdit.MinorEdit,
				RevisionMessage = PageToEdit.RevisionMessage
			};
			await _wikiPages.Add(page);

			if (page.Revision == 1 || !PageToEdit.MinorEdit)
			{
				_publisher.SendGeneralWiki(
					$"Page {Path} {(page.Revision > 1 ? "edited" : "created")} by {User.Identity.Name}",
					$"({PageToEdit.RevisionMessage}): ",
					$"{BaseUrl}/{Path}");
			}

			return Redirect("/" + page.PageName);
		}

		private async Task<bool> UserNameExists(string path)
		{
			var userName = WikiHelper.ToUserName(path);
			return await _db.Users.Exists(userName);
		}
	}
}
