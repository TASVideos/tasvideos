using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("timezone-picker", TagStructure = TagStructure.WithoutEndTag, Attributes = "asp-for")]
public class TimeZonePickerTagHelper : TagHelper
{
	[HtmlAttributeName("asp-for")]
	public ModelExpression For { get; set; } = null!;

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		var forType = For.ModelExplorer.ModelType;
		if (forType != typeof(string))
		{
			throw new ArgumentException($"Invalid property type {forType}, {nameof(For)} must be a string");
		}

		var modelName = For.Name;
		var modelValue = (string)For.ModelExplorer.Model;
		if (string.IsNullOrWhiteSpace(modelValue))
		{
			modelValue = TimeZoneInfo.Utc.Id;
		}

		output.TagMode = TagMode.StartTagAndEndTag;

		output.TagName = "select";
		output.Attributes.Add("id", modelName);
		output.Attributes.Add("name", modelName);

		var groups = TimeZoneInfo
			.GetSystemTimeZones()
			.Select(t => t.BaseUtcOffset)
			.Distinct()
			.ToList();

		var availableTimezones = TimeZoneInfo
			.GetSystemTimeZones()
			.Select(t => new
			{
				t.BaseUtcOffset,
				t.Id,
				Selected = t.Id == modelValue
			})
			.OrderBy(t => t.BaseUtcOffset)
			.ThenBy(t => t.Id)
			.ToList();

		foreach (var optgroup in groups)
		{
			output.Content.AppendHtml($"<optgroup {Attr("label", optgroup.ToString())}>");

			var options = availableTimezones.Where(t => t.BaseUtcOffset == optgroup);

			foreach (var option in options)
			{
				output.Content.AppendHtml($@"
						<option {(option.Selected ? "selected" : "")} {Attr("value", option.Id)} {Attr("data-offset", option.BaseUtcOffset.TotalMinutes.ToString())}>
							{Text(option.Id)}
						</option>
					");
			}

			output.Content.AppendHtml("</optgroup>");
		}
	}
}
