using System.Collections.Generic;
using System.Linq;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	using W = IronMeta.Matcher.MatchItem<char, INode>;
	partial class Wiki
	{
		private static string Str(W content)
		{
			return new string(content.Inputs.ToArray());
		}
		private static INode MakeText(string content)
		{
			return new Text(content);
		}
		private static INode MakeText(W content)
		{
			return new Text(Str(content));
		}
		private static INode MakeElt(string tag)
		{
			return new Element(tag);
		}
		private static INode MakeElt(string tag, W children)
		{
			return new Element(tag, children.Results);
		}
		private static KeyValuePair<string, string> Attr(string name, W value)
		{
			return new KeyValuePair<string, string>(name, ((Text)value.Results.Single()).Content);
		}
		private static INode MakeElt(string tag, W children, params KeyValuePair<string, string>[] attrs)
		{
			return new Element(tag, attrs, children.Results);
		}
		private static INode MakeIf(W condition, W children)
		{
			return new IfModule(((Text)condition.Results.Single()).Content, children.Results);
		}
		private static INode MakeModule(W condition)
		{
			return new Module(((Text)condition.Results.Single()).Content);
		}
	}
}
