using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

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
		void WriteHtml(TextWriter w);
		INode Clone();
	}
	public interface INodeWithChildren : INode
	{
		List<INode> Children { get; }
	}

	public class Text : INode
	{
		public NodeType Type => NodeType.Text;
		public string Content { get; }
		public int CharStart { get; }
		public int CharEnd { get; set; }
		public Text(int charStart, string content)
		{
			CharStart = charStart;
			Content = content;
		}
		public void WriteHtml(TextWriter w)
		{
			foreach (var c in Content)
			{
				switch (c)
				{
					case '<':
						w.Write("&lt;");
						break;
					case '&':
						w.Write("&amp;");
						break;
					case '@':
						w.Write("&#64;");
						break;
					default:
						w.Write(c);
						break;
				}
			}
		}
		public INode Clone()
		{
			return (Text)MemberwiseClone();
		}
	}
	
	public class Element : INodeWithChildren
	{
		private static readonly Regex AllowedTagNames = new Regex("^[a-z0-9]+$");
		private static readonly Regex AllowedAttributeNames = new Regex("^[a-z\\-]+$");
		private static readonly HashSet<string> VoidTags = new HashSet<string>
		{
			"area", "base", "br", "col", "embed", "hr", "img", "input",
			"keygen", "link", "meta", "param", "source", "track", "wbr"
		};
		public NodeType Type => NodeType.Element;
		public List<INode> Children { get; private set; } = new List<INode>();
		public IDictionary<string, string> Attributes { get; private set; } = new Dictionary<string, string>();
		public string Tag { get; }
		public int CharStart { get; }
		public int CharEnd { get; set; }
		public Element(int charStart, string tag)
		{
			if (!AllowedTagNames.IsMatch(tag))
			{
				throw new InvalidOperationException("Invalid tag name");
			}
			if (tag == "script" || tag == "style")
			{
				// we don't escape for these
				throw new InvalidOperationException("Unsupported tag!");
			}
			CharStart = charStart;
			Tag = tag;
		}
		public Element(int charStart, string tag, IEnumerable<INode> children)
			:this(charStart, tag)
		{
			Children.AddRange(children);
		}
		public Element(int charStart, string tag, IEnumerable<KeyValuePair<string, string>> attributes, IEnumerable<INode> children)
			:this(charStart, tag, children)
		{
			foreach (var kvp in attributes)
				Attributes.Add(kvp.Key, kvp.Value);
		}
		public void WriteHtml(TextWriter w)
		{	if (VoidTags.Contains(Tag) && Children.Count > 0)
			{
				throw new InvalidOperationException("Void tag with child content!");
			}
			w.Write('<');
			w.Write(Tag);
			foreach (var a in Attributes)
			{
				if (!AllowedAttributeNames.IsMatch(a.Key))
				{
					throw new InvalidOperationException("Invalid attribute name");
				}
				w.Write(' ');
				w.Write(a.Key);
				w.Write("=\"");
				foreach (var c in a.Value)
				{
					switch (c)
					{
						case '<':
							w.Write("&lt;");
							break;
						case '&':
							w.Write("&amp;");
							break;
						case '"':
							w.Write("&quot;");
							break;
						case '@':
							w.Write("&#64;");
							break;
						default:
							w.Write(c);
							break;
					}
				}
				w.Write('"');
			}
			if (VoidTags.Contains(Tag))
			{
				w.Write(" />");
			}
			else
			{
				w.Write('>');
				foreach (var c in Children)
					c.WriteHtml(w);
				w.Write("</");
				w.Write(Tag);
				w.Write('>');
			}
		}
		public INode Clone()
		{
			var ret = (Element)MemberwiseClone();
			ret.Children = Children.Select(c => c.Clone()).ToList();
			ret.Attributes = new Dictionary<string, string>(Attributes);
			return ret;
		}
	}

	public class IfModule : INodeWithChildren
	{
		public NodeType Type => NodeType.IfModule;
		public List<INode> Children { get; private set; } = new List<INode>();
		public string Condition { get; }
		public int CharStart { get; }
		public int CharEnd { get; set; }
		public IfModule(int charStart, string condition)
		{
			CharStart = charStart;
			Condition = condition;
		}
		public IfModule(int charStart, string condition, IEnumerable<INode> children)
			: this(charStart, condition)
		{
			Children.AddRange(children);
		}
		public void WriteHtml(TextWriter w)
		{
			// razor stuff
			w.Write("@if(Html.WikiCondition(");
			Escape.WriteCSharpString(w, Condition);
			w.Write(")){<text>");
			foreach (var c in Children)
				c.WriteHtml(w);
			w.Write("</text>}");
		}
		public INode Clone()
		{
			var ret = (IfModule)MemberwiseClone();
			ret.Children = Children.Select(c => c.Clone()).ToList();
			return ret;
		}
	}

	public class Module : INode
	{
		private static readonly Dictionary<string, string> ModuleNameMaps = new Dictionary<string, string>
		{
			["listsubpages"] = "ListSubPages",
			["__wikilink"] = "WikiLink",
			["wikiorphans"] = "WikiOrphans",
			["brokenlinks"] = "BrokenLinks",
			["wikitextchangelog"] = "WikiTextChangeLog",
			["usergetwikiname"] = "UserGetWikiName",
			["activetab"] = "ActiveTab",
			["wikigetcurrenteditlink"] = "CurrentEditLink",
			["user_name"] = "UserName",
			["listparents"] = "ListParents",
			["youtube"] = "Youtube",
			["platformframerates"] = "PlatformFramerates",
			["frontpagesubmissionlist"] = "FrontpageSubmissionList",
			["displayminimovie"] = "DisplayMiniMovie",
			["tabularmovielist"] = "TabularMovieList",
			["topicfeed"] = "TopicFeed",
			["gamename"] = "GameName",
			["gamesubpages"] = "GameSubPages",
			["awards"] = "Awards",
			["usermovies"] = "UserMovies"
		};
		public NodeType Type => NodeType.Module;
		public string Text { get; }
		public int CharStart { get; }
		public int CharEnd { get; set; }
		public Module(int charStart, int charEnd, string text)
		{
			CharStart = charStart;
			CharEnd = charEnd;
			Text = text;
		}
		public void WriteHtml(TextWriter w)
		{
			var pp = Text.Split(new[] { '|' }, 2);
			var moduleName = pp[0];
			var moduleParams = pp.Length > 1 ? pp[1] : "";
			if (ModuleNameMaps.TryGetValue(moduleName?.ToLower(), out string realModuleName))
			{
				w.Write("@(await Component.InvokeAsync(");
				Escape.WriteCSharpString(w, realModuleName);
				w.Write(", new { pageData = Model, pp = ");
				Escape.WriteCSharpString(w, moduleParams);
				w.Write(" }))");
			}
			else
			{
				var div = new Element(CharStart, "div") { CharEnd = CharEnd };
				div.Children.Add(new Text(CharStart, "Unknown module " + moduleName) { CharEnd = CharEnd });
				div.Attributes["class"] = "module-error";
				div.WriteHtml(w);
			}
		}
		public INode Clone()
		{
			return (Module)MemberwiseClone();
		}
	}
}
