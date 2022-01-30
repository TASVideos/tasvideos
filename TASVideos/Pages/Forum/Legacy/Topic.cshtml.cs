using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Legacy;

// Handles legacy forum links to viewTopic.php
[AllowAnonymous]
public class TopicModel : BaseForumModel
{
	private readonly IForumService _forumService;

	public TopicModel(IForumService forumService)
	{
		_forumService = forumService;
	}

	[FromQuery]
	public int? P { get; set; }

	[FromQuery]
	public int? T { get; set; }

	[FromRoute]
	public int? Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		if (!P.HasValue && !T.HasValue && !Id.HasValue)
		{
			return NotFound();
		}

		if (P.HasValue)
		{
			var model = await _forumService.GetPostPosition(P.Value, User.Has(PermissionTo.SeeRestrictedForums));
			if (model == null)
			{
				return NotFound();
			}

			return BasePageRedirect(
				"/Forum/Topics/Index",
				new
				{
					Id = model.TopicId,
					CurrentPage = model.Page,
					Highlight = P
				});
		}

		return BasePageRedirect("/Forum/Topics/Index", new { Id = T ?? Id });
	}
}
