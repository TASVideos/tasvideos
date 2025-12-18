using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class CancelLinkTagHelper(IHtmlGenerator generator, IHttpContextAccessor httpContextAccessor) : AnchorTagHelper(generator)
{
	public string? BtnClassOverride { get; set; }

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

		base.Process(context, output);
		output.AddCssClass("btn");

		output.AddCssClass(string.IsNullOrEmpty(BtnClassOverride)
			? "btn-secondary"
			: BtnClassOverride);

		if (innerContentIsBlank)
		{
			output.Content.AppendHtml("<i class=\"fa fa-times\"></i> Cancel");
		}
	}

	private string? GetReturnUrl() => httpContextAccessor.HttpContext?.Request.ReturnUrl();
}
