using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TASVideos.WikiEngine.AST;

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

		public static NewParser.SyntaxException ParsePageForErrors(string content)
		{
			try
			{
				NewParser.Parse(content);
				return null;
			}
			catch (NewParser.SyntaxException e)
			{
				return e;
			}
		}

		public static void RenderHtmlDynamic(string content, TextWriter w, IWriterHelper h)
		{
			List<INode> results;
			try
			{
				results = NewParser.Parse(content);
			}
			catch (NewParser.SyntaxException e)
			{
				results = Builtins.MakeErrorPage(content, e);
			}

			foreach (var r in results)
			{
				r.WriteHtmlDynamic(w, h);
			}			
		}

		public static IEnumerable<WikiLinkInfo> GetAllWikiLinks(string content)
		{
			try
			{
				var results = NewParser.Parse(content);
				return NodeUtils.GetAllWikiLinks(content, results).Where(l => !l.Link.Contains("user:"));
			}
			catch (NewParser.SyntaxException)
			{
				return Enumerable.Empty<WikiLinkInfo>();
			}
		}

		// this probably shouldn't be here, but to avoid circular dependencies it is
		public static string NormalizeWikiPageName(string link)
		{
			if (link.StartsWith("user:"))
			{
				link = "HomePages/" + link.Substring(5);
			}
			else
			{
				// Support links like [Judge Guidelines] linking to [JudgeGuidelines]
				// We don't do this replacement if link is a user module in order to support users with spaces such as Walker Boh
				link = link.Replace(" ", "");
			}

			if (link.EndsWith(".html", true, CultureInfo.InvariantCulture))
			{
				link = link.Substring(0, link.Length - 5);
			}

			link = link.Trim('/');
			link = string.Join("/", link.Split('/').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
			return link;
		}
	}
}
