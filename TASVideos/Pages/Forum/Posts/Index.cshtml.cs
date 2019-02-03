using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Posts
{
	// TODO: how to do this without a redirect
	[AllowAnonymous]
	public class IndexModel : BaseForumModel
	{
		public IndexModel(ApplicationDbContext db) : base(db)
		{
		}

		[FromRoute]
		public int Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var model = await GetPostPosition(Id, User.Has(PermissionTo.SeeRestrictedForums));
			if (model == null)
			{
				return NotFound();
			}

			return RedirectToPage(
				"/Forum/Topics/Index", 
				new
				{
					Id = model.TopicId, 
					Highlight = Id, 
					CurrentPage = model.Page
				});
		}
	}
}
