using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum;

public class BaseForumModel : BasePageModel
{
	protected static readonly List<SelectListItem> TopicTypeList = Enum
		.GetValues<ForumTopicType>()
		.ToDropDown();

	protected static readonly List<SelectListItem> MoodList = Enum
		.GetValues<ForumPostMood>()
		.Select(m => new SelectListItem
		{
			Value = ((int)m).ToString(),
			Text = $"{(int)m}: {m.EnumDisplayName()}",
			Group = m >= ForumPostMood.AltNormal ? AltGroup : StandardGroup
		})
		.ToList();

	private static readonly SelectListGroup StandardGroup = new() { Name = "Standard" };
	private static readonly SelectListGroup AltGroup = new() { Name = "Alternate" };

	public List<SelectListItem> Moods => MoodList;
	public List<SelectListItem> TopicTypes => TopicTypeList;

	public new IActionResult NotFound()
	{
		return RedirectToPage("/Forum/NotFound");
	}
}
