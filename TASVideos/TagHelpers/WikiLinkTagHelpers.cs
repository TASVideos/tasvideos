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
		output.TagName = "a";
		Page = "/Publications/View";
		RouteValues.Add(nameof(Pages.Publications.ViewModel.Id), Id.ToString());
		base.Process(context, output);
	}
}

public class SubLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		Page = "/Submissions/View";
		RouteValues.Add(nameof(Pages.Submissions.ViewModel.Id), Id.ToString());
		base.Process(context, output);
	}
}

public class GameLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public int Id { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		Page = "/Games/Index";
		RouteValues.Add(nameof(Pages.Games.IndexModel.Id), Id.ToString());
		base.Process(context, output);
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
		RouteValues.Add(nameof(Pages.Users.ProfileModel.UserName), Username);
		await base.ProcessAsync(context, output);
	}
}
