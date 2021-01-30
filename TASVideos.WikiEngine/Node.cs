using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TASVideos.WikiEngine.AST
{
	public enum NodeType
	{
		Text,
		Element,
		IfModule,
		Module
	}

	public interface INode
	{
		[JsonConverter(typeof(StringEnumConverter))]
		NodeType Type { get; }
		int CharStart { get; }
		int CharEnd { get; set; }
		INode Clone();
		void WriteHtmlDynamic(TextWriter w, WriterContext h);

		/// <summary>
		/// Get the combined text content of this Node.  May not return useful values for foreign components (Modules).
		/// </summary>
		string InnerText(IWriterHelper h);

		/// <summary>
		/// Debugging output of all of the data in this node.
		/// </summary>
		void DumpContentDescriptive(TextWriter w, string padding);

		/// <summary>
		/// Clones this node for use in a TOC.  Some things like anchors are removed.
		/// </summary>
		IEnumerable<INode> CloneForToc();
	}

	/// <summary>
	/// Provides helpers that the wiki engine needs to render page results
	/// </summary>
	public interface IWriterHelper
	{
		/// <summary>
		/// Check the condition for one of the wiki language's conditional markups
		/// </summary>
		/// <param name="condition">The condition; eg `CanEditPages` or `!CanJudgeMovies`</param>
		/// <returns>The value of the condition for the current user context.</returns>
		bool CheckCondition(string condition);

		/// <summary>
		/// Run a ViewComponent ("module" in wiki lingo)
		/// </summary>
		/// <param name="w">The stream that the module should output its markup results to.</param>
		/// <param name="name">The name of the module.</param>
		/// <param name="pp">The module's parameter text, direct from the markup.</param>
		void RunViewComponent(TextWriter w, string name, string pp);
	}

	/// <summary>
	/// A fake IWriterHelper which can give "good enough" results if a static context is needed.
	/// </summary>
	public class NullWriterHelper : IWriterHelper
	{
		public bool CheckCondition(string condition)
		{
			return false;
		}

		public void RunViewComponent(TextWriter w, string name, string pp)
		{
		}

		private NullWriterHelper()
		{
		}

		public static readonly NullWriterHelper Instance = new ();
	}

	public interface INodeWithChildren : INode
	{
		List<INode> Children { get; }
	}

	/// <summary>
	/// Used internally by nodes to assist them in writing output.
	/// </summary>
	public class WriterContext
	{
		private readonly List<KeyValuePair<Regex, string>> _tableAttributeRunners = new ();

		public IWriterHelper Helper { get; }
		public WriterContext(IWriterHelper helper)
		{
			Helper = helper;
		}

		/// <summary>
		/// Adds a table style filter expression for later use in table cells.
		/// </summary>
		/// <param name="pp">The raw parameter text from the markup.</param>
		/// <returns></returns>
		public bool AddTdStyleFilter(string pp)
		{
			var regex = ParamHelper.GetValueFor(pp, "pattern");
			var style = ParamHelper.GetValueFor(pp, "style");
			if (string.IsNullOrWhiteSpace(regex) || string.IsNullOrWhiteSpace(style))
			{
				return false;
			}

			try
			{
				// TODO: What's actually going on with these @s?
				if (regex[0] == '@')
				{
					regex = regex[1..];
				}

				if (regex[^1] == '@')
				{
					regex = regex[..^1];
				}

				var r = new Regex(regex, RegexOptions.None, TimeSpan.FromSeconds(1));
				_tableAttributeRunners.Add(new KeyValuePair<Regex, string>(r, style));
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Run all existing td style filters.
		/// </summary>
		/// <param name="text">The raw text to evalute against the style filters.</param>
		/// <returns>A style attribute value, or null if no filters matched.</returns>
		public string? RunTdStyleFilters(string text)
		{
			foreach (var (key, value) in _tableAttributeRunners)
			{
				if (key.Match(text).Success)
				{
					return value;
				}
			}

			return null;
		}
	}
}
