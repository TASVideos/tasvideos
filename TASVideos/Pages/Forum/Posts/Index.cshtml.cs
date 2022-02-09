using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Posts;

// TODO: how to do this without a redirect
[AllowAnonymous]
public class IndexModel : BaseForumModel
{
	private readonly IForumService _forumService;
	public IndexModel(IForumService forumService)
	{
		_forumService = forumService;
	}

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var model = await _forumService.GetPostPosition(Id, User.Has(PermissionTo.SeeRestrictedForums));
		if (model == null)
		{
			return NotFound();
		}

		return RedirectToPage(
			"/Forum/Topics/Index",
			null,
			new
			{
				Id = model.TopicId,
				CurrentPage = model.Page,
				Highlight = Id
			},
			Id.ToString());
	}
}
