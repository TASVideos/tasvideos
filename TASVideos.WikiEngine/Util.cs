using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace TASVideos.WikiEngine
{
	public static class Util
	{
		public static string DebugParseWikiPage(string content)
		{
			try
			{
				var results = NewParser.Parse(content);
				return JsonConvert.SerializeObject(results, Formatting.Indented);
			}
			catch (NewParser.SyntaxException e)
			{
				return JsonConvert.SerializeObject(new
				{
					Error = e.Message
				}, Formatting.Indented);
			}
		}
		public static void DebugWriteHtml(string content, TextWriter w)
		{
			try
			{
				var results = NewParser.Parse(content);
				foreach (var r in results)
					r.WriteHtml(w);
			}
			catch (NewParser.SyntaxException e)
			{
				w.Write($"<!-- ERROR {e.Message} -->");
			}
		}

		public static void RenderRazor(string pageName, string content, TextWriter w)
		{
			var results = NewParser.Parse(content);
			w.WriteLine(@"@model WikiPage");
			w.Write(@"@{ Layout = (string)ViewData[""Layout""]; }");

			foreach (var r in results)
				r.WriteHtml(w);
		}

		public static IEnumerable<NewParser.WikiLinkInfo> GetAllWikiLinks(string content)
		{
			return NewParser.GetAllWikiLinks(content);
		}


		public static string RenderUserModuleLink(string value)
		{
			if (value.StartsWith("user:"))
			{
				return TryConvertToValidPageName(value.Replace("user:", "HomePages/"));
			}

			return value;
		}

		public static string TryConvertToValidPageName(string pageName)
		{
			if (string.IsNullOrWhiteSpace(pageName))
			{
				return "";
			}

			pageName = Regex.Replace(
				pageName
					.Replace(".html", "")
					.Trim('/'),
				@"\s",
				"");

			return ConvertProperCase(pageName);
		}

		public static IEnumerable<int> AllIndexesOf(string str, string searchstring)
		{
			int minIndex = str.IndexOf(searchstring);
			while (minIndex != -1)
			{
				yield return minIndex;
				minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length);
			}
		}

		private static string ConvertProperCase(string pageName)
		{
			pageName = pageName?.Trim('/');
			pageName = char.ToUpper(pageName[0]) + pageName.Substring(1);

			var slashes = AllIndexesOf(pageName, "/");
			foreach (var slash in slashes)
			{
				pageName = pageName.Substring(0, slash + 1)
					+ char.ToUpper(pageName[slash + 1])
					+ pageName.Substring(slash + 2);
			}

			return pageName;
		}
	}
}
