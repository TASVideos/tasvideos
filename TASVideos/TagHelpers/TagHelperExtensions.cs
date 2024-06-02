using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public static partial class TagHelperExtensions
{
	public static void AddCssClass(this TagHelperOutput output, string className)
	{
		var existingClass = output.Attributes.FirstOrDefault(a => a.Name == "class");
		if (existingClass is not null)
		{
			output.Attributes.Remove(existingClass);
			output.Attributes.Add(new TagHelperAttribute("class", existingClass.Value + " " + className));
		}
		else
		{
			output.Attributes.Add(new TagHelperAttribute("class", className));
		}
	}

	public static string GetString(this IHtmlContent content)
	{
		var writer = new StringWriter();
		content.WriteTo(writer, HtmlEncoder.Default);
		return writer.ToString();
	}

	private static readonly Regex ValidAttributeName = ValidAttributeNameRegex();

	/// <summary>
	/// Return an HTML attribute <code>name=&quot;value&quot;</code> pair with appropriate escaping.
	/// </summary>
	public static string Attr(string name, string value)
	{
		if (!ValidAttributeName.IsMatch(name))
		{
			throw new ArgumentException($"Attribute name `{name}` contains invalid characters", nameof(name));
		}

		var sb = new StringBuilder(name.Length + value.Length + 3);
		sb.Append(name);
		sb.Append("=\"");
		foreach (var c in value)
		{
			switch (c)
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

	/// <summary>
	/// Returns raw text HTML escaped suitable for use in a regular element body
	/// </summary>
	public static string Text(string text)
	{
		var sb = new StringBuilder(text.Length);
		foreach (var c in text)
		{
			switch (c)
			{
				case '<':
					sb.Append("&lt;");
					break;
				case '&':
					sb.Append("&amp;");
					break;
				default:
					sb.Append(c);
					break;
			}
		}

		return sb.ToString();
	}

	[GeneratedRegex("^[^\t\n\f \\/>\"'=]+$")]
	private static partial Regex ValidAttributeNameRegex();
}
