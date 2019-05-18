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

		public static IEnumerable<InternalLinkInfo> GetAllInternalLinks(string content)
		{
			try
			{
				var results = NewParser.Parse(content);
				return NodeUtils.GetAllInternalLinks(content, results);
			}
			catch (NewParser.SyntaxException)
			{
				return Enumerable.Empty<InternalLinkInfo>();
			}
		}
	}
}
