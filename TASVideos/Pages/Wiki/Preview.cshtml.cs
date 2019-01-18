using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;
using TASVideos.WikiEngine;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	public class PreviewModel : BasePageModel
	{
		public PreviewModel(
			UserTasks userTasks)
			: base(userTasks)
		{
		}

		public string Markup { get; set; }

		public IActionResult OnPost()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			Markup = input;
			return Page();
		}
	}
}
