using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class AddLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		var task = output.GetChildContentAsync();
		task.Wait();
		SetOutput(context, output, task.Result.IsEmptyOrWhiteSpace);
	}

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		=> SetOutput(context, output, (await output.GetChildContentAsync()).IsEmptyOrWhiteSpace);

	private void SetOutput(TagHelperContext context, TagHelperOutput output, bool innerContentIsBlank)
	{
		output.TagName = "a";
		base.Process(context, output);
		output.AddCssClass("btn");
		output.AddCssClass("btn-primary");
		if (innerContentIsBlank)
		{
			output.Content.AppendHtml("<i class=\"fa fa-plus\"></i> Add");
		}
	}
}
