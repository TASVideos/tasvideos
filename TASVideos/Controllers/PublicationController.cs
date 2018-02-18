using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;


namespace TASVideos.Controllers
{
	public class PublicationController : BaseController
	{
		private readonly PublicationTasks _publicationTasks;

		public PublicationController(
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
		}

		[AllowAnonymous]
		public IActionResult Index()
		{
			return RedirectToAction(nameof(List));
		}

		[AllowAnonymous]
		public async Task<IActionResult> List(string query)
		{
			var model = await _publicationTasks.GetMovieList(new PublicationSearchModel());
			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> View(int id)
		{
			var model = await _publicationTasks.GetPublicationForDisplay(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Download(int id)
		{
			(var fileBytes, var fileName) = await _publicationTasks.GetPublicationMovieFile(id);
			if (fileBytes.Length > 0)
			{
				return File(fileBytes, MediaTypeNames.Application.Octet, $"{fileName}.zip");
			}

			return BadRequest();
		}
	}
}
