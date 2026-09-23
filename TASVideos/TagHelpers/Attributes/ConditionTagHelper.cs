using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement(Attributes = nameof(Condition))]
public class ConditionTagHelper : TagHelper
{
	public override int Order => 10; // output suppression must happen last
	public bool Condition { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (!Condition)
		{
			output.SuppressOutput();
		}
	}
}

[HtmlTargetElement(Attributes = nameof(Condition))]
public class ConditionResultSetterTagHelper : ConditionTagHelper
{
	public override int Order => -10; // this makes sure this runs before other tag helpers. they can avoid creating antiforgery tokens that don't get used.

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		context.SetConditionResult(Condition);
	}
}
