using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Helpers;

namespace TASVideos.TagHelpers;

public class PubLinkTagHelper : TagHelper
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		output.Attributes.Add("href", $"/{Id}M");
	}
}

public class SubLinkTagHelper : TagHelper
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		output.Attributes.Add("href", $"/{Id}S");
	}
}

public class GameLinkTagHelper : TagHelper
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		output.Attributes.Add("href", $"/{Id}G");
	}
}

public class WikiLinkTagHelper : TagHelper
{
	public string PageName { get; set; } = "";

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		var pageName = PageName.Trim('/');
		var submissionId = SubmissionHelper.IsRawSubmissionLink(PageName);
		if (submissionId.HasValue)
		{
			pageName = $"{submissionId}S";
		}

		var publicationId = SubmissionHelper.IsRawPublicationLink(PageName);
		if (publicationId.HasValue)
		{
			pageName = $"{publicationId}M";
		}

		var gameId = SubmissionHelper.IsRawGamePageLink(PageName);
		if (gameId.HasValue)
		{
			pageName = $"{gameId}G";
		}

		output.TagName = "a";
		output.Attributes.Add("href", $"/{pageName}");
		output.Content.Clear();
		output.Content.AppendHtml(pageName);
	}
}

public class ProfileLinkTagHelper(IHtmlGenerator htmlGenerator) : AnchorTagHelper(htmlGenerator)
{
	public string? Username { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		var task = output.GetChildContentAsync();
		task.Wait();
		SetOutput(context, output, innerContentIsBlank: task.Result.IsEmptyOrWhiteSpace);
	}

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		=> SetOutput(context, output, innerContentIsBlank: (await output.GetChildContentAsync()).IsEmptyOrWhiteSpace);

	private void SetOutput(TagHelperContext context, TagHelperOutput output, bool innerContentIsBlank)
	{
		if (innerContentIsBlank)
		{
			output.Content.Clear();
			output.Content.Append(Username ?? "");
		}

		output.TagName = "a";
		Page = "/Users/Profile";
		RouteValues.Add("UserName", Username);
		base.Process(context, output);
	}
}
