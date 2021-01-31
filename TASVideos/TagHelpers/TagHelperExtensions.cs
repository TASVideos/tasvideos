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

		private static readonly Regex ValidAttributeName = new Regex("^[^\t\n\f \\/>\"'=]+$");

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
			foreach (var c in name)
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
		/// <param name="text"></param>
		/// <returns></returns>
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

		// This is overly restrictive.  If you want to name your js identifiers fÖÖbar, feel free to change it.
		private static readonly Regex ValidJsIdentifier = new Regex("^[a-zA-Z_\\-$][a-zA-Z_\\-$0-9]+$");

		/// <summary>
		/// Returns a JS identifier suitable for use inside a script tag, after verifying that all characters in it are sane
		/// </summary>
		public static string JsIdentifier(string identifier)
		{
			if (!ValidJsIdentifier.IsMatch(identifier))
			{
				throw new ArgumentException($"Identifier `{identifier}` contains invalid characters", nameof(identifier));
			}

			return identifier;
		}

		/// <summary>
		/// Returns a value serialized to javascript, suitable for inclusion in a script tag.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string JsValue(object? value)
		{
			// Newtonsoft by default never escapes `/`; we always escape it to avoid stray </script>s.
			return Newtonsoft.Json.JsonConvert.SerializeObject(value).Replace("/", "\\/");
		}
	}
}
