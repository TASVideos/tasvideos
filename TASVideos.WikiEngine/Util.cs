using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static class Util
	{
		// ReSharper disable once UnusedMember.Global
		public static string DebugParseWikiPage(string content)
		{
			try
			{
				var results = NewParser.Parse(content);
				return JsonConvert.SerializeObject(results, Formatting.Indented);
			}
			catch (NewParser.SyntaxException e)
			{
				return JsonConvert.SerializeObject(
					new
					{
						Error = e.Message
					}, Formatting.Indented);
			}
		}

		public static NewParser.SyntaxException? ParsePageForErrors(string content)
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
				r.WriteHtmlDynamic(w, new WriterContext(h));
			}
		}

		/// <summary>
		/// Returns all the referrals to other site pages that exist in the given wiki markup.
		/// </summary>
		public static IEnumerable<InternalLinkInfo> GetReferrals(string content)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(content))
				{
					return Enumerable.Empty<InternalLinkInfo>();
				}

				var parsed = NewParser.Parse(content);
				var results = NodeUtils.GetAllInternalLinks(content, parsed);
				return results
					.Where(l => !string.IsNullOrWhiteSpace(l.Link.Split('#')[0]))
					.Select(l => new InternalLinkInfo(l.Link.Split('#')[0], l.Excerpt))
					.ToList();
			}
			catch (NewParser.SyntaxException)
			{
				return Enumerable.Empty<InternalLinkInfo>();
			}
		}
	}
}
