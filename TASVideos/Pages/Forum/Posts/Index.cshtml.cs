namespace TASVideos.Pages.Forum.Posts;

// TODO: how to do this without a redirect
[AllowAnonymous]
public class IndexModel(IForumService forumService) : BaseForumModel
{
	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var model = await forumService.GetPostPosition(Id, User.Has(PermissionTo.SeeRestrictedForums));
		if (model is null)
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
