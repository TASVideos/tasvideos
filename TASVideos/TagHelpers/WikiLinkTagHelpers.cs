using System.Threading.Tasks;
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
			pageName = $"/{submissionId}S";
		}

		var publicationId = SubmissionHelper.IsRawPublicationLink(PageName);
		if (publicationId.HasValue)
		{
			pageName = $"/{publicationId}M";
		}

		var gameId = SubmissionHelper.IsRawGamePageLink(PageName);
		if (gameId.HasValue)
		{
			pageName = $"/{gameId}G";
		}

		output.TagName = "a";
		output.Attributes.Add("href", pageName);
		output.Content.Clear();
		output.Content.AppendHtml(pageName.Trim('/'));
	}
}

[HtmlTargetElement("profile-link")]
public class ProfileLinkTagHelper : AnchorTagHelper
{
	public ProfileLinkTagHelper(IHtmlGenerator htmlGenerator)
		: base(htmlGenerator)
	{
	}

	public string Username { get; set; } = "";

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var innerContent = await output.GetChildContentAsync();
		if (innerContent.IsEmptyOrWhiteSpace)
		{
			output.Content.Clear();
			output.Content.Append(Username);
		}

		output.TagName = "a";
		Page = "/Users/Profile";
		RouteValues.Add("UserName", Username);
		await base.ProcessAsync(context, output);
	}
}
