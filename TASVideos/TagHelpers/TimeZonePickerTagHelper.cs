using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("timezone-picker", TagStructure = TagStructure.WithoutEndTag)]
	public class TimeZonePickerTagHelper : TagHelper
	{
		[HtmlAttributeName("asp-for")]
		public ModelExpression For { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var forType = For.ModelExplorer.ModelType;
			if (forType != typeof(string))
			{
				throw new ArgumentException($"Invalid property type {forType}, {nameof(For)} must be a string");
			}

			var modelName = For.ModelExplorer.Metadata.PropertyName;
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
					t.DisplayName,
					Selected = t.Id == modelValue
				})
				.OrderBy(t => t.BaseUtcOffset)
				.ThenBy(t => t.DisplayName)
				.ToList();

			foreach (var optgroup in groups)
			{
				output.Content.AppendHtml($"<optgroup label='{optgroup.ToString()}'>");

				var options = availableTimezones.Where(t => t.BaseUtcOffset == optgroup);
				
				foreach (var option in options)
				{
					output.Content.AppendHtml($"<option {(option.Selected ? "selected" : "")} value='{option.Id}' data-offset='{option.BaseUtcOffset.TotalMinutes}'>{option.DisplayName}</option>");
				}

				output.Content.AppendHtml("</optgroup>");
			}
		}
	}
}
