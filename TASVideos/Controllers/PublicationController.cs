using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
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

		public IActionResult Index()
		{
			return RedirectToAction(nameof(List));
		}

		public IActionResult List()
		{
			return new EmptyResult();
		}

		public async Task<IActionResult> View(int id)
		{
			var model = await _publicationTasks.GetPublicationForDisplay(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}
	}
}
