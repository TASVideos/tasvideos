using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class DiffModel : BasePageModel
	{
		private readonly WikiTasks _wikiTasks;

		public DiffModel(
			WikiTasks wikiTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
		}

		[FromQuery]
		public string Path { get; set; }

		[FromQuery]
		public int? FromRevision { get; set; }

		[FromQuery]
		public int? ToRevision { get; set; }

		public WikiDiffModel Diff { get; set; }

		public async Task OnGet()
		{
			Path = Path?.Trim('/');

			Diff = FromRevision.HasValue && ToRevision.HasValue
				? await _wikiTasks.GetPageDiff(Path, FromRevision.Value, ToRevision.Value)
				: await _wikiTasks.GetLatestPageDiff(Path);
		}

		public async Task<IActionResult> OnGetDiffData(string path, int fromRevision, int toRevision)
		{
			var data = await _wikiTasks.GetPageDiff(path.Trim('/'), fromRevision, toRevision);
			return new JsonResult(data);
		}
	}
}
