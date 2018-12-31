using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[RequireEdit]
	public class EditModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;

		public EditModel(
			IWikiPages wikiPages,
			ExternalMediaPublisher publisher,
			UserTasks userTasks)
			: base(userTasks)
		{
			_wikiPages = wikiPages;
			_publisher = publisher;
		}

		[FromQuery]
		public string Path { get; set; }

		[BindProperty]
		public WikiEditModel PageToEdit { get; set; } = new WikiEditModel();

		public IActionResult OnGet()
		{
			Path = Path?.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(Path))
			{
				return Home();
			}

			PageToEdit = new WikiEditModel
			{
				Markup = _wikiPages.Page(Path)?.Markup ?? ""
			};

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!WikiHelper.IsValidWikiPageName(Path))
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
					$"{PageToEdit.RevisionMessage}",
					$"{BaseUrl}/{Path}");
			}

			return Redirect("/" + page.PageName);
		}
	}
}
