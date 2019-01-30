using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Html;
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

		public static string GetString(this IHtmlContent content)
		{
		    var writer = new StringWriter();
		    content.WriteTo(writer, HtmlEncoder.Default);
		    return writer.ToString();
		}
	}
}
