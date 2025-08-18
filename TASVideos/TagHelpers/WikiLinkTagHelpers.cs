using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Helpers;

namespace TASVideos.TagHelpers;

public class PubLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		Page = $"/{Id}M";
		base.Process(context, output);
	}
}

public class SubLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		Page = $"/{Id}S";
		base.Process(context, output);
	}
}

public class GameLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		Page = $"/{Id}G";
		base.Process(context, output);
	}
}

public class WikiLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
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

		Page = $"/{pageName}";
		base.Process(context, output);
		output.Content.Clear();
		output.Content.AppendHtml(pageName);
	}
}

public class ProfileLinkTagHelper(IHtmlGenerator htmlGenerator) : AnchorTagHelper(htmlGenerator)
{
	public string? Username { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
		=> ProcessAsync(context, output).Wait();

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var innerContent = await output.GetChildContentAsync();
		if (innerContent.IsEmptyOrWhiteSpace)
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
