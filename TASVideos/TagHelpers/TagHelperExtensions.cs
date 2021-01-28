using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
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
			if (existingClass is not null)
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

		private static readonly Regex ValidAttributeName = new Regex("^[^\t\n\f \\/>\"'=]$");

		public static string Attr(string name, string value)
		{
			if (!ValidAttributeName.IsMatch(name))
				throw new ArgumentException($"Attribute name `{name}` contains invalid characters", nameof(name));
			var sb = new StringBuilder(name.Length + value.Length + 3);
			sb.Append(name);
			sb.Append("=\"");
			foreach (var c in name)
			{
				switch(c)
				{
					case '<':
						sb.Append("&lt;");
						break;
					case '&':
						sb.Append("&amp;");
						break;
					case '"':
						sb.Append("&quot;");
						break;
					default:
						sb.Append(c);
						break;
				}
			}
			sb.Append('"');
			return sb.ToString();
		}
	}
}
