using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class IconTagHelper : TagHelper
{
	public string? Path { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (!string.IsNullOrWhiteSpace(Path))
		{
			var basePath = Path.Split('.')[0];
			var classIconPath2X = $"{basePath}-2x.png";
			var classIconPath4X = $"{basePath}-4x.png";

			output.TagName = "img";
			output.Attributes.Add("style", "width: 18px");
			output.Attributes.Add("src", classIconPath2X);
			output.Attributes.Add("srcset", $"/{Path} .5x, /{classIconPath2X} 1x, /{classIconPath4X} 2x");
		}
	}
}
