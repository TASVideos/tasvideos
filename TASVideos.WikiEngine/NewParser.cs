using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public class NewParser
	{
		public class SyntaxException : Exception
		{
			public int TextLocation { get; }
			public SyntaxException(string msg, int textLocation) : base(msg)
			{
				TextLocation = textLocation;
			}
		}

		private List<INode> _output = new List<INode>();
		private List<INodeWithChildren> _stack = new List<INodeWithChildren>();
		private int _currentTextStart = -1;
		private StringBuilder _currentText = new StringBuilder();
		private string _input;
		private int _index = 0;
		private delegate void ActionType(NewParser p);
		private bool _parsingInline = false;

		private void Abort(string msg)
		{
			throw new SyntaxException(msg, _index); 
		}
		private bool Eat(char c)
		{
			if (EOF() || c != _input[_index])
				return false;
			_index++;
			return true;
		}
		private bool Eat(string s)
		{
			int j;
			for (j = 0; j < s.Length; j++)
			{
				if (_index + j >= _input.Length || s[j] != _input[_index + j])
					return false;
			}
			_index += j;
			return true;
		}
		private char Eat()
		{
			return _input[_index++];
		}
		private bool EatEOL()
		{
			return Eat("\r\n") || Eat('\r') || Eat('\n');
		}
		private bool EOF()
		{
			return _index == _input.Length;
		}
		private string EatToBracket()
		{
			var ret = new StringBuilder();
			for (var i = 1; i > 0;)
			{
				if (EOF())
					Abort("Unexpected EOF parsing text in []");
				if (EatEOL())
					Abort("Unexpected EOL parsing text in []");
				var c = Eat();
				if (c == '[')
					i++;
				else if (c == ']')
					i--;
				if (i > 0)
					ret.Append(c);
			}
			return ret.ToString();
		}
		private string EatClassText()
		{
			var ret = new StringBuilder();
			if (!EOF())
				Eat(' '); // OK if this fails?
			while (!EOF() && !EatEOL())
				ret.Append(Eat());
			return ret.ToString();
		}
		private string EatTabName()
		{
			var ret = new StringBuilder();
			while (!EOF() && !EatEOL() && !Eat('%'))
				ret.Append(Eat());
			DiscardLine();
			return ret.ToString();
		}
		private string EatSrcEmbedText()
		{
			var ret = new StringBuilder();
			while (!EOF() && !Eat("%%END_EMBED"))
				ret.Append(Eat());
			DiscardLine();
			return ret.ToString();
		}
		private string EatBullets()
		{
			var ret = new StringBuilder();
			while (!EOF())
			{
				if (Eat('*'))
					ret.Append('*');
				else if (Eat('#'))
					ret.Append('#');
				else
					break;
			}
			return ret.ToString();
		}
		private void DiscardLine()
		{
			while (!EOF() && !EatEOL())
				Eat();
		}
		private void AddText(char c)
		{
			if (_currentText.Length == 0)
				_currentTextStart = _index;
			_currentText.Append(c);
		}
		private void AddText(string s)
		{
			if (_currentText.Length == 0)
				_currentTextStart = _index;
			_currentText.Append(s);
		}
		private void FinishText()
		{
			if (_currentText.Length > 0)
			{
				var t = new Text(_currentTextStart, _currentText.ToString()) { CharEnd = _index };
				_currentText.Clear();
				if (_stack.Count > 0)
					_stack[_stack.Count - 1].Children.Add(t);
				else
					_output.Add(t);
			}
		}
		private bool TryPop(string tag)
		{
			for (var i = _stack.Count - 1; i >= 0; i--)
			{
				var e = _stack[i];
				if (e.Type == NodeType.Element && ((Element)e).Tag == tag)
				{
					FinishText();
					for (var j = i; j < _stack.Count; j++)
						_stack[j].CharEnd = _index;
					_stack.RemoveRange(i, _stack.Count - i);
					return true;
				}
			}
			return false;
		}
		private bool TryPopTab()
		{
			// when popping an individual tab, we don't want to pop through a tabset.  only TAB_END does that
			for (var i = _stack.Count - 1; i >= 0; i--)
			{
				var e = _stack[i];
				if (e.Type == NodeType.Element)
				{
					var tag = ((Element)e).Tag;
					if (tag == "htabs" || tag == "vtabs")
						return false;
					if (tag == "tab")
					{
						FinishText();
						for (var j = i; j < _stack.Count; j++)
							_stack[j].CharEnd = _index;
						_stack.RemoveRange(i, _stack.Count - i);
						return true;
					}
				}
			}
			return false;			
		}
		private bool TryPopTabs()
		{
			for (var i = _stack.Count - 1; i >= 0; i--)
			{
				var e = _stack[i];
				if (e.Type == NodeType.Element)
				{
					var tag = ((Element)e).Tag;
					if (tag == "htabs" || tag == "vtabs")
					{
						FinishText();
						for (var j = i; j < _stack.Count; j++)
							_stack[j].CharEnd = _index;
						_stack.RemoveRange(i, _stack.Count - i);
						return true;
					}
				}
			}
			return false;
		}
		private void Pop(string tag)
		{
			if (!TryPop(tag))
				throw new InvalidOperationException("Internal parser error: Pop " + tag);
		}
		private void PopOrPush(string tag)
		{
			if (!TryPop(tag))
				Push(tag);
		}
		private void Push(INodeWithChildren n)
		{
			AddNonChild(n);
			_stack.Add(n);
		}
		private void Push(string tag)
		{
			Push(new Element(_index, tag));
		}
		private bool TryPopIf()
		{
			for (var i = _stack.Count - 1; i >= 0; i--)
			{
				var e = _stack[i];
				if (e.Type == NodeType.IfModule)
				{
					FinishText();
					for (var j = i; j < _stack.Count; j++)
						_stack[j].CharEnd = _index;
					_stack.RemoveRange(i, _stack.Count - i);
					return true;
				}
			}
			return false;
		}
		private void AddNonChild(INode n)
		{
			FinishText();
			if (_stack.Count > 0)
				_stack[_stack.Count - 1].Children.Add(n);
			else
				_output.Add(n);
		}
		private void AddNonChild(IEnumerable<INode> n)
		{
			FinishText();
			if (_stack.Count > 0)
				_stack[_stack.Count - 1].Children.AddRange(n);
			else
				_output.AddRange(n);			
		}
		private void ClearBlockTags()
		{
			// any block level tag that isn't explicitly closed in markup
			// except ul / ol, which have special handling
			if (!TryPop("h1")
				&& !TryPop("h2")
				&& !TryPop("h3")
				&& !TryPop("h4")
				&& !TryPop("table")
				&& !TryPop("dl")
				&& !TryPop("blockquote")
				&& !TryPop("pre")
				&& !TryPop("p"))
			{ }
		}
		private bool In(string tag)
		{
			foreach (var e in _stack)
			{
				if (e.Type == NodeType.Element && ((Element)e).Tag == tag)
					return true;
			}
			return false;
		}
		private void SwitchToInline()
		{
			if (_parsingInline)
				throw new InvalidOperationException("Internal parser error");
			_parsingInline = true;
		}
		private void SwitchToLine()
		{
			if (!_parsingInline)
				throw new InvalidOperationException("Internal parser error");
			_parsingInline = false;
		}

		private void ParseInlineText()
		{
			if (Eat("__"))
				PopOrPush("b");
			else if (Eat("''"))
				PopOrPush("em");
			else if (Eat("---"))
				PopOrPush("del");
			else if (Eat("(("))
				Push("small");
			else if (Eat("))"))
			{
				if (!TryPop("small"))
					AddText("))");
			}
			else if (Eat("{{"))
				Push("tt");
			else if (Eat("}}"))
			{
				if (!TryPop("tt"))
					AddText("}}");
			}
			else if (Eat("««"))
				Push("q");
			else if (Eat("»»"))
			{
				if (!TryPop("q"))
					AddText("»»");
			}
			else if (Eat("%%%"))
			{
				while (Eat('%')) { }
				AddNonChild(new Element(_index, "br") { CharEnd = _index });
			}
			else if (Eat("[["))
				AddText('[');
			else if (Eat("]]"))
				AddText(']');
			else if (Eat("[if:"))
			{
				Push(new IfModule(_index, EatToBracket()));
			}
			else if (Eat("[endif]"))
			{
				if (!TryPopIf())
					Abort("[endif] missing corresponding [if:]");
			}
			else if (Eat('['))
			{
				var start = _index;
				var content = EatToBracket();
				AddNonChild(Builtins.MakeModule(start, _index, content));
			}
			else if (In("dt") && Eat(':'))
			{
				Pop("dt");
				Push("dd");
			}
			else if (In("th") && Eat("||"))
			{
				Pop("th");
				if (EatEOL())
					SwitchToLine();
				else
					Push("th");
			}
			else if (In("td") && Eat('|'))
			{
				Pop("td");
				if (EatEOL())
					SwitchToLine();
				else
					Push("td");
			}
			else if (EatEOL())
			{
				AddText('\n');
				SwitchToLine();
			}
			else
				AddText(Eat());
		}

		private string ComputeExistingBullets()
		{
			var ret = new StringBuilder();
			for (var i = 0; i < _stack.Count; i++)
			{
				var e = _stack[i];
				if (e.Type == NodeType.Element)
				{
					switch (((Element)e).Tag)
					{
						case "ul": ret.Append('*'); break;
						case "ol": ret.Append('#'); break;
					}
				}
			}
			return ret.ToString();
		}
		private bool StartLineLists()
		{
			var old = ComputeExistingBullets();
			var nue = EatBullets();
			int keep;
			for (keep = 0; keep < old.Length; keep++)
			{
				if (nue.Length <= keep || old[keep] != nue[keep])
					break;
			}
			for (var i = old.Length - 1; i >= keep; i--)
			{
				Pop(old[i] == '*' ? "ul" : "ol");
			}
			for (var i = keep; i < nue.Length; i++)
			{
				if (i == 0)
					ClearBlockTags();
				else if (!In("li"))
					Push("li");
				Push(nue[i] == '*' ? "ul" : "ol");
				Push("li");
			}
			if (nue.Length > 0)
			{
				if (nue == old)
				{
					Pop("li");
					Push("li");
				}
				return true;
			}
			return false;
		}

		private void ParseStartLine()
		{
			if (StartLineLists())
			{
				SwitchToInline();
			}
			else if (Eat("%%QUOTE_END"))
			{
				if (!TryPop("quote"))
					Abort("Mismatched %%QUOTE_END");
			}
			else if (Eat("%%QUOTE"))
			{
				var author = EatClassText();
				ClearBlockTags();
				var e = new Element(_index, "quote");
				if (author != "")
					e.Attributes["data-author"] = author;
				Push(e);
			}
			else if (Eat("%%DIV_END"))
			{
				if (!TryPop("div"))
					Abort("Mismatched %%DIV_END");
			}
			else if (Eat("%%DIV"))
			{
				var className = EatClassText();
				ClearBlockTags();
				var e = new Element(_index, "div");
				if (className != "")
					e.Attributes["class"] = className;
				Push(e);		
			}
			else if (Eat("%%TAB "))
			{
				var name = EatTabName();
				ClearBlockTags();
				if (!In("vtabs") && !In("htabs"))
					Push("vtabs");
				else
					TryPopTab();
				var e = new Element(_index, "tab");
				e.Attributes["data-name"] = name;
				Push(e);
			}
			else if (Eat("%%TAB_START"))
			{
				DiscardLine();
				ClearBlockTags();
				Push("vtabs");
			}
			else if (Eat("%%TAB_HSTART"))
			{
				DiscardLine();
				ClearBlockTags();
				Push("htabs");
			}
			else if (Eat("%%TAB_END"))
			{
				DiscardLine();
				if (!TryPopTabs())
					Abort("Mismatched %%TAB_END%%");
			}
			else if (Eat("[if:"))
			{
				ClearBlockTags();
				Push(new IfModule(_index, EatToBracket()));
			}
			else if (Eat("[endif]"))
			{
				if (!TryPopIf())
					Abort("[endif] missing corresponding [if:]");
			}
			else if (Eat("----"))
			{
				while (Eat('-')) { }
				ClearBlockTags();
				AddNonChild(new Element(_index, "hr") { CharEnd = _index });
			}
			else if (Eat("!!!!"))
			{
				ClearBlockTags();
				Push("h1");
				SwitchToInline();
			}
			else if (Eat("!!!"))
			{
				ClearBlockTags();
				Push("h2");
				SwitchToInline();
			}
			else if (Eat("!!"))
			{
				ClearBlockTags();
				Push("h3");
				SwitchToInline();
			}
			else if (Eat("!"))
			{
				ClearBlockTags();
				Push("h4");
				SwitchToInline();
			}
			else if (Eat("%%TOC%%"))
			{
				DiscardLine();
				ClearBlockTags();
				AddNonChild(new Element(_index, "toc") { CharEnd = _index });
			}
			else if (Eat("||"))
			{				
				if (In("tbody"))
					Pop("table");
				if (!In("table"))
				{
					ClearBlockTags();
					Push("table");
				}
				if (!In("thead"))
					Push("thead");
				TryPop("tr");
				Push("tr");
				Push("th");
				SwitchToInline();
			}
			else if (Eat('|'))
			{
				if (In("thead"))
					Pop("thead");
				if (!In("table"))
				{
					ClearBlockTags();
					Push("table");
				}
				if (!In("tbody"))
					Push("tbody");
				TryPop("tr");
				Push("tr");
				Push("td");
				SwitchToInline();
			}
			else if (Eat(';'))
			{
				if (!In("dl"))
				{
					ClearBlockTags();
					Push("dl");
				}
				Push("dt");
				SwitchToInline();
			}
			else if (Eat("%%SRC_EMBED"))
			{
				var lang = EatClassText();
				ClearBlockTags();
				var e = new Element(_index, "code");
				if (lang != "")
					e.Attributes["class"] = "language-" + lang;
				e.Children.Add(new Text(_index, EatSrcEmbedText()) { CharEnd = _index });
				e.CharEnd = _index;
				var ret = new Element(e.CharStart, "pre") { CharEnd = e.CharEnd };
				ret.Children.Add(e);
				AddNonChild(ret);
			}
			else if (Eat('>'))
			{
				if (!In("blockquote"))
				{
					ClearBlockTags();
					Push("blockquote");
				}
				SwitchToInline();
			}
			else if (Eat(' '))
			{
				if (!In("pre"))
				{
					ClearBlockTags();
					Push("pre");
				}
				SwitchToInline();
			}
			else if (EatEOL())
			{
				ClearBlockTags();
			}
			else
			{
				if (!In("p"))
				{
					ClearBlockTags();
					Push("p");
				}
				SwitchToInline();
			}
		}

		private void ParseLoop()
		{
			while (!EOF())
			{
				if (_parsingInline)
					ParseInlineText();
				else
					ParseStartLine();
			}
			FinishText();
		}

		private static void ReplaceTabs(List<INode> n)
		{
			Replace(n,
				e => e.Type == NodeType.Element && (((Element)e).Tag == "htabs" || ((Element)e).Tag == "vtabs"),
				e => Builtins.MakeTabs((Element)e));
		}

		private static void ReplaceTocs(List<INode> n)
		{
			Replace(n,
				e => e.Type == NodeType.Element && ((Element)e).Tag == "toc",
				e => GenerateToc(n, e.CharStart));
		}

		private static void Replace(List<INode> input, Func<INode, bool> predicate, Func<INode, INode> transform)
		{
			for (var i = 0; i < input.Count; i++)
			{
				if (predicate(input[i]))
				{
					input[i] = transform(input[i]);
				}
				var cc = input[i] as INodeWithChildren;
				if (cc != null)
					Replace(cc.Children, predicate, transform);
			}
		}

		private static IEnumerable<INode> Find(List<INode> input, Func<INode, bool> predicate)
		{
			foreach (var n in input)
			{
				if (predicate(n))
					yield return n;
				var cc = n as INodeWithChildren;
				if (cc != null)
				{
					foreach (var c in Find(cc.Children, predicate))
						yield return c;
				}
			}
		}

		private List<WikiLinkInfo> GetAllWikiLinks()
		{
			return Find(_output, e => e.Type == NodeType.Module && ((Module)e).Text.StartsWith("__wikiLink|"))
				.Select(e =>
				{
					var text = ((Module)e).Text;
					var link = text.Substring(11);
					var si = Math.Max(e.CharStart - 20, 0);
					var se = Math.Min(e.CharEnd + 20, _input.Length);
					return new WikiLinkInfo
					{
						Link = link,
						Excerpt = _input.Substring(si, se - si)
					};
				})
				.ToList();
		}

		private static readonly HashSet<string> Headings = new HashSet<string>
		{
			// h1, h5, h6 are not involved in TOC generation
			"h2", "h3", "h4"
		};
		
		private static int HeadingId = 0;

		private static Element GenerateToc(List<INode> document, int charStart)
		{
			var headings = Find(document, e => e.Type == NodeType.Element && Headings.Contains(((Element)e).Tag))
				.Cast<Element>()
				.ToList();

			var ret = new Element(charStart, "div");
			ret.Attributes["class"] = "toc";
			var stack = new Stack<Element>();
			{
				var ul = new Element(charStart, "ul");
				ret.Children.Add(ul);
				stack.Push(ul);
			}
			var pos = 2;
			foreach (var h in headings)
			{
				var i = h.Tag[1] - '0'; // 2, 3, or 4?
				while (i > pos)
				{
					var next = new Element(charStart, "ul");
					var li = new Element(charStart, "li", new[] { next });
					stack.Peek().Children.Add(li);
					stack.Push(next);
					pos++;
				}
				while (i < pos)
				{
					stack.Pop();
					pos--;
				}
				{
					var id = "heading-" + Interlocked.Increment(ref HeadingId);

					var link = new Element(charStart, "a");
					var li = new Element(charStart, "li", new[] { link });
					link.Attributes["href"] = "#" + id;
					link.Children.AddRange(h.Children.Select(c => c.Clone()));
					stack.Peek().Children.Add(li);

					var anchor = new Element(charStart, "a");
					anchor.Attributes["id"] = id;
					h.Children.Add(anchor);
				}
			}
			return ret;
		}

		public class WikiLinkInfo
		{
			public string Link { get; set; }
			public string Excerpt { get; set; }
		}

		public static List<WikiLinkInfo> GetAllWikiLinks(string content)
		{
			try
			{
				var p = new NewParser { _input = content };
				p.ParseLoop();
				return p.GetAllWikiLinks();
			}
			catch (SyntaxException)
			{
				// What to do here?
				return new List<WikiLinkInfo>();
			}
		}

		public static List<INode> Parse(string content)
		{
			var p = new NewParser { _input = content };
			try
			{
				p.ParseLoop();
			}
			catch (SyntaxException e)
			{
				return MakeErrorPage(content, e);
			}
			ReplaceTabs(p._output);
			ReplaceTocs(p._output);
			return p._output;
		}

		public static List<INode> MakeErrorPage(string content, SyntaxException e)
		{
			var ret = new List<INode>();

			var head = new Element(0, "h1", new[] { new Text(0, "Syntax Error") });
			ret.Add(head);

			var elt = new Element(0, "pre");
			var i = e.TextLocation;
			elt.Children.Add(new Text(0, content.Substring(0, i)));
			var marker = new Element(i, "span", new[] { new Element(i, "span", new[] { new Text(i, e.Message) }) });
			marker.Attributes["class"] = "error-marker";
			elt.Children.Add(marker);
			elt.Children.Add(new Text(i, content.Substring(i)));
			ret.Add(elt);

			return ret;
		}
	}
}
