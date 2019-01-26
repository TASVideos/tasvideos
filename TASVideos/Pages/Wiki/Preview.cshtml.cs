using System.IO;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	public class PreviewModel : BasePageModel
	{
		private readonly IWikiPages _pages;

		public PreviewModel(
			UserTasks userTasks, IWikiPages pages)
			: base(userTasks)
		{
			_pages = pages;
		}

		public string Markup { get; set; }

		[FromQuery]
		public int? Id { get; set; }

		public WikiPage PageData { get; set; }

		public IActionResult OnPost()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			Markup = input;
			if (Id.HasValue)
			{
				PageData = _pages.Revision(Id.Value);
			}

			return Page();
		}
	}
}
