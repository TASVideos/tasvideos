using System.Collections.Generic;
using System.Threading.Tasks;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
	public class DeletedPagesModel : BasePageModel
	{
		private readonly WikiTasks _wikiTasks;

		public DeletedPagesModel(
			WikiTasks wikiTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
		}

		public IEnumerable<DeletedWikiPageDisplayModel> DeletedPages { get; set; } = new List<DeletedWikiPageDisplayModel>();

		public async Task OnGet()
		{
			DeletedPages = await _wikiTasks.GetDeletedPages();
		}
	}
}
