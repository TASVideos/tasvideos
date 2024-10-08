using System.Text.Json;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static class Util
{
	// ReSharper disable once UnusedMember.Global
	public static string DebugParseWikiPage(string content)
	{
		try
		{
			var results = NewParser.Parse(content, isUGC: false);
			return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
		}
		catch (NewParser.SyntaxException e)
		{
			return JsonSerializer.Serialize(
				new { Error = e.Message },
				new JsonSerializerOptions { WriteIndented = true });
		}
	}

	public static NewParser.SyntaxException? ParsePageForErrors(string content, bool isUGC)
	{
		try
		{
			NewParser.Parse(content, isUGC: isUGC);
			return null;
		}
		catch (NewParser.SyntaxException e)
		{
			return e;
		}
	}

	public static async Task RenderHtmlAsync(string content, TextWriter w, IWriterHelper h)
	{
		List<INode> results;
		try
		{
			results = NewParser.Parse(content, isUGC: h.IsUGC);
		}
		catch (NewParser.SyntaxException e)
		{
			results = Builtins.MakeErrorPage(content, e);
		}

		var ctx = new WriterContext(h);
		foreach (var r in results)
		{
			await r.WriteHtmlAsync(w, ctx);
		}
	}

	public static async Task RenderTextAsync(string content, TextWriter w, IWriterHelper h)
	{
		List<INode> results;
		try
		{
			results = NewParser.Parse(content, isUGC: h.IsUGC);
		}
		catch (NewParser.SyntaxException e)
		{
			results = Builtins.MakeErrorPage(content, e);
		}

		var ctx = new WriterContext(h);
		foreach (var r in results)
		{
			await r.WriteTextAsync(w, ctx);
		}
	}

	public static async Task RenderMetaDescriptionAsync(string content, StringBuilder sb, IWriterHelper h)
	{
		List<INode> results;
		try
		{
			results = NewParser.Parse(content, isUGC: h.IsUGC);
		}
		catch (NewParser.SyntaxException e)
		{
			results = Builtins.MakeErrorPage(content, e);
		}

		var ctx = new WriterContext(h);
		foreach (var r in results)
		{
			if (sb.Length >= SiteGlobalConstants.MetaDescriptionLength)
			{
				break;
			}

			await r.WriteMetaDescriptionAsync(sb, ctx);
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
				return [];
			}

			var parsed = NewParser.Parse(content, isUGC: false/*doesn't matter*/);
			var results = NodeUtils.GetAllInternalLinks(content, parsed);
			return results
				.Where(l => !string.IsNullOrWhiteSpace(l.Link.Split('#')[0]))
				.Select(l => new InternalLinkInfo(l.Link.Split('#')[0], l.Excerpt))
				.ToList();
		}
		catch (NewParser.SyntaxException)
		{
			return [];
		}
	}
}
