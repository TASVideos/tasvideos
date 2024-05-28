using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class AlertTagHelper : TagHelper
{
	public bool Dismissible { get; set; }

	public virtual string Type { get; set; } = "info";

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var content = (await output.GetChildContentAsync()).GetContent();
		output.TagName = "div";
		output.Attributes.Add("role", "alert");
		output.AddCssClass($"alert alert-{Type} text-center");
		if (Dismissible)
		{
			output.AddCssClass("alert-dismissible");
			output.Content.SetHtmlContent(
				$"<button type='button' class='btn-close float-end' data-bs-dismiss='alert' aria-label='close'></button>{content}");
		}
	}
}

public class InfoAlertTagHelper : AlertTagHelper
{
	public override string Type { get; set; } = "info";
}

public class WarningAlertTagHelper : AlertTagHelper
{
	public override string Type { get; set; } = "warning";
}

public class DangerAlertTagHelper : AlertTagHelper
{
	public override string Type { get; set; } = "danger";
}
