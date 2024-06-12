using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.TagHelpers;

public class MoodPreviewTagHelper : TagHelper
{
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public AvatarUrls? Avatar { get; set; }

	public ForumPostMood CurrentMood { get; set; } = ForumPostMood.Normal;

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "div";

		if (ViewContext.HttpContext.User.Has(PermissionTo.UseMoodAvatars) && (Avatar?.HasMoods ?? false))
		{
			ViewContext.ViewData.UseMoodPreview();
			var src = Avatar.ToMoodUrl(CurrentMood);
			output.Content.AppendHtml(
			$"""
			<img class="mt-2" id="mood-img" data-base="{Avatar.MoodBase}" src="{src}" />
			""");
		}
	}
}
