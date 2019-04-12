using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
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

	[HtmlTargetElement("profile-link")]
	public class ProfileLinkTagHelper : AnchorTagHelper
	{
		public ProfileLinkTagHelper(IHtmlGenerator htmlGenerator) : base(htmlGenerator)
		{
		}

		public string Username { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "a";
			Page = "/Users/Profile";
			RouteValues.Add("UserName", Username);
			base.Process(context, output);
		}
	}
}
