using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	public static class TagHelperExtensions
	{
		public static void AddCssClass(this TagHelperOutput output, string className)
		{
			var existingClass = output.Attributes.FirstOrDefault(a => a.Name == "class");
			var cssClass = existingClass?.Value.ToString() ?? "";
			cssClass += $" {className}";
			var ta = new TagHelperAttribute("class", cssClass);
			if (existingClass != null)
			{
				output.Attributes.Remove(existingClass);
			}

			output.Attributes.Add(ta);
		}
	}
}
