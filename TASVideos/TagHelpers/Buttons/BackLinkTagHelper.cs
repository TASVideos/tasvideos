using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class BackLinkTagHelper(IHtmlGenerator generator, IHttpContextAccessor httpContextAccessor) : AnchorTagHelper(generator)
{
	public string? NameOverride { get; set; }

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";

		var returnUrl = GetReturnUrl();
		if (!string.IsNullOrWhiteSpace(returnUrl))
		{
			output.Attributes.SetAttribute("href", returnUrl);
			Page = null;
			Action = null;
			Area = null;
			PageHandler = null;
			Fragment = null;
			RouteValues = new Dictionary<string, string>();
		}

		await base.ProcessAsync(context, output);
		output.AddCssClass("btn");
		output.AddCssClass("btn-secondary");

		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			var name = string.IsNullOrEmpty(NameOverride) ? "Back" : NameOverride;
			output.Content.AppendHtml($"<i class=\"fa fa-arrow-left\"></i> {name}");
		}
	}

	private string? GetReturnUrl() => httpContextAccessor.HttpContext?.Request.ReturnUrl();
}
