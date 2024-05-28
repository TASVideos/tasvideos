using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class ColumnTagHelper : TagHelper
{
	public int? Xs { get; set; }
	public int? Sm { get; set; }
	public int? Md { get; set; }
	public int? Lg { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		Validate();
		output.TagName = "div";

		var classList = new List<string>();
		if (Xs.HasValue)
		{
			classList.Add($"col-xs-{Xs}");
		}

		if (Sm.HasValue)
		{
			classList.Add($"col-sm-{Sm}");
		}

		if (Md.HasValue)
		{
			classList.Add($"col-md-{Md}");
		}

		if (Lg.HasValue)
		{
			classList.Add($"col-lg-{Lg}");
		}

		if (!classList.Any())
		{
			classList.Add("col-12");
		}

		foreach (var c in classList)
		{
			output.AddCssClass(c);
		}
	}

	private void Validate()
	{
		if (Xs is < 1 or > 12)
		{
			throw new ArgumentException($"{nameof(Xs)} must be in the range of 1-12");
		}

		if (Sm is < 1 or > 12)
		{
			throw new ArgumentException($"{nameof(Sm)} must be in the range of 1-12");
		}

		if (Md is < 1 or > 12)
		{
			throw new ArgumentException($"{nameof(Md)} must be in the range of 1-12");
		}

		if (Lg is < 1 or > 12)
		{
			throw new ArgumentException($"{nameof(Lg)} must be in the range of 1-12");
		}
	}
}
