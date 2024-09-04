using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement(Attributes = "backup-content")]
public class BackupContentTagHelper : TagHelper
{
	[HtmlAttributeName("backup-content")]
	public bool BackupContent { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (BackupContent)
		{
			output.Attributes.Add("data-backup-content", "true");
			ViewContext.ViewData.UseBackupText();
		}
	}
}
