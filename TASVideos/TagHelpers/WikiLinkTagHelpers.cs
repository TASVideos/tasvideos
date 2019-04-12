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
}
