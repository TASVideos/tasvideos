using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class WikiEditHelperTagHelper : TagHelper
{
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		ViewContext.ViewData.UseWikiEditHelper();
		output.TagName = "div";
		output.AddCssClass("row");
		output.Content.AppendHtml(
			$$$"""
			<div class="col-12">
				<div class="btn-group mt-1 mb-1">
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="__,__" data-key="b"><strong>b</strong></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="'',''" data-key="i"><i>i</i></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="---,---" data-key="k"><s>s</s></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="((,))"><small>sm</small></button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="{{,}}">tt</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="----,">ruler</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[user:{{{ViewContext.HttpContext.User.Name()}}}]: ," data-akey="u">user</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[module:youtube|v=,]">youtube</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[module:frames|amount=,]">frames</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[Forum/Posts/,]">Link Post</button>
					<button type="button" class="btn btn-info btn-sm border-dark" tabindex="-1" data-fmt="[Forum/Topics/,]">Link Topic</button>
				</div>
			</div>
			""");
	}
}
