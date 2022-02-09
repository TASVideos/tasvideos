using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public class NewParser
{
	public class SyntaxException : Exception
	{
		public int TextLocation { get; }
		public SyntaxException(string msg, int textLocation)
			: base(msg)
		{
			TextLocation = textLocation;
		}
	}

	private readonly List<INode> _output = new();
	private readonly List<INodeWithChildren> _stack = new();
	private readonly StringBuilder _currentText = new();
	private int _currentTextStart = -1;
	private readonly string _input;
	private int _index;
	private bool _parsingInline;

	private NewParser(string input)
	{
		_input = input;
	}

	private static void Abort(string msg, int from)
	{
		throw new SyntaxException(msg, from);
	}

	private bool Eat(char c)
	{
		if (Eof() || c != _input[_index])
		{
			return false;
		}

		_index++;
		return true;
	}

	private bool Eat(string s)
	{
		int j;
		for (j = 0; j < s.Length; j++)
		{
			if (_index + j >= _input.Length || s[j] != _input[_index + j])
			{
				return false;
			}
		}

		_index += j;
		return true;
	}

	private string? Eat(Regex r)
	{
		var match = r.Match(_input, _index);
		if (!match.Success)
		{
			return null;
		}

		var ret = match.Value;
		_index += ret.Length;
		return ret;
	}

	private char Eat()
	{
		return _input[_index++];
	}

	private bool EatEol()
	{
		return Eat("\r\n") || Eat('\r') || Eat('\n');
	}

	private bool EatWhitespaceOnlyToEolEof()
	{
		// TODO: parser combinators
		int j;
		for (j = 0; _index + j < _input.Length; j++)
		{
			var c = _input[_index + j];
			switch (c)
			{
				case ' ':
					continue;
				case '\r':
					if (j < _input.Length - 1 && _input[_index + j + 1] == '\n')
					{
						j++;
					}

					goto case '\n';
				case '\n':
					j++;
					goto end;
				default:
					return false;
			}
		}

	end:
		_index += j;
		return true;
	}

	private bool Eof()
	{
		return _index == _input.Length;
	}

	private string EatToBracket()
	{
		var from = _index;
		var ret = new StringBuilder();
		for (var i = 1; i > 0;)
		{
			if (Eof())
			{
				Abort("Unexpected EOF parsing text in []", from);
			}

			if (EatEol())
			{
				Abort("Unexpected EOL parsing text in []", from);
			}

			var c = Eat();
			if (c == '[')
			{
				i++;
			}
			else if (c == ']')
			{
				i--;
			}

			if (i > 0)
			{
				ret.Append(c);
			}
		}

		return ret.ToString();
	}

	private string EatClassText()
	{
		var ret = new StringBuilder();
		if (!Eof())
		{
			Eat(' '); // OK if this fails?
		}

		while (!Eof() && !EatEol())
		{
			ret.Append(Eat());
		}

		return ret.ToString();
	}

	private string EatTabName()
	{
		var ret = new StringBuilder();
		var sawEndOfLine = false;
		while (!Eof() && !(sawEndOfLine = EatEol()) && !Eat('%'))
		{
			ret.Append(Eat());
		}

		if (!sawEndOfLine)
		{
			DiscardLine();
		}

		return ret.ToString();
	}

	private string EatSrcEmbedText()
	{
		var ret = new StringBuilder();
		while (!Eof() && !Eat("%%END_EMBED"))
		{
			ret.Append(Eat());
		}

		DiscardLine();
		return ret.ToString();
	}

	private string EatBullets()
	{
		var ret = new StringBuilder();
		while (!Eof())
		{
			if (Eat('*'))
			{
				ret.Append('*');
			}
			else if (Eat('#'))
			{
				ret.Append('#');
			}
			else
			{
				break;
			}
		}

		return ret.ToString();
	}

	private int EatPipes()
	{
		var start = _index;
		while (!Eof() && Eat('|'))
		{
		}

		return _index - start;
	}

	private void DiscardLine()
	{
		while (!Eof() && !EatEol())
		{
			Eat();
		}
	}

	private void AddText(char c)
	{
		if (_currentText.Length == 0)
		{
			_currentTextStart = _index;
		}

		_currentText.Append(c);
	}

	private void AddText(string s)
	{
		if (_currentText.Length == 0)
		{
			_currentTextStart = _index;
		}

		_currentText.Append(s);
	}

	private void FinishText()
	{
		if (_currentText.Length > 0)
		{
			var t = new Text(_currentTextStart, _currentText.ToString()) { CharEnd = _index };
			_currentText.Clear();
			if (_stack.Count > 0)
			{
				_stack[^1].Children.Add(t);
			}
			else
			{
				_output.Add(t);
			}
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
				{
					_stack[j].CharEnd = _index;
				}

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
				{
					return false;
				}

				if (tag == "tab")
				{
					FinishText();
					for (var j = i; j < _stack.Count; j++)
					{
						_stack[j].CharEnd = _index;
					}

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
					{
						_stack[j].CharEnd = _index;
					}

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
		{
			throw new InvalidOperationException("Internal parser error: Pop " + tag);
		}
	}

	private void PopOrPush(string tag)
	{
		if (!TryPop(tag))
		{
			Push(tag);
		}
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
				{
					_stack[j].CharEnd = _index;
				}

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
		{
			_stack[^1].Children.Add(n);
		}
		else
		{
			_output.Add(n);
		}
	}

	private void AddNonChild(IEnumerable<INode> n)
	{
		FinishText();
		if (_stack.Count > 0)
		{
			_stack[^1].Children.AddRange(n);
		}
		else
		{
			_output.AddRange(n);
		}
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
		{
		}
	}

	private bool In(string tag)
	{
		foreach (var e in _stack)
		{
			if (e.Type == NodeType.Element && ((Element)e).Tag == tag)
			{
				return true;
			}
		}

		return false;
	}

	private void SwitchToInline()
	{
		if (_parsingInline)
		{
			throw new InvalidOperationException("Internal parser error");
		}

		_parsingInline = true;
	}

	private void SwitchToLine()
	{
		if (!_parsingInline)
		{
			throw new InvalidOperationException("Internal parser error");
		}

		_parsingInline = false;
	}

	private static readonly Regex Url = new(@"\G
			https?:\/\/
			(
				[A-Za-z0-9\-._~!$&'()*+,;=:@\/]
				|
				%[A-Fa-f0-9]{2}
			)+
			(
				\?
				(
					[A-Za-z0-9\-._~!$&'()*+,;=:@\/]
					|
					%[A-Fa-f0-9]{2}
				)+
			)?
			(
				\#
				(
					[A-Za-z0-9\-._~!$&'()*+,;=:@\/]
					|
					%[A-Fa-f0-9]{2}
				)+
			)?
		", RegexOptions.IgnorePatternWhitespace);

	private void ParseInlineText()
	{
		int tmp;
		string? url;

		if (Eat("__"))
		{
			PopOrPush("b");
		}
		else if (Eat("''"))
		{
			PopOrPush("em");
		}
		else if (Eat("---"))
		{
			PopOrPush("del");
		}
		else if (Eat("(("))
		{
			Push("small");
		}
		else if (Eat("))"))
		{
			if (!TryPop("small"))
			{
				AddText("))");
			}
		}
		else if (Eat("{{"))
		{
			Push("code");
		}
		else if (Eat("}}"))
		{
			if (!TryPop("code"))
			{
				AddText("}}");
			}
		}
		else if (Eat("««"))
		{
			Push("q");
		}
		else if (Eat("»»"))
		{
			if (!TryPop("q"))
			{
				AddText("»»");
			}
		}
		else if (Eat("⸢⸢"))
		{
			Push("sup");
		}
		else if (Eat("⸣⸣"))
		{
			if (!TryPop("sup"))
			{
				AddText("⸣⸣");
			}
		}
		else if (Eat("⸤⸤"))
		{
			Push("sub");
		}
		else if (Eat("⸥⸥"))
		{
			if (!TryPop("sub"))
			{
				AddText("⸥⸥");
			}
		}
		else if (Eat("%%%"))
		{
			while (Eat('%'))
			{
			}

			AddNonChild(new Element(_index, "br") { CharEnd = _index });
		}
		else if (Eat("[["))
		{
			AddText('[');
		}
		else if (Eat("]]"))
		{
			AddText(']');
		}
		else if (Eat("[if:"))
		{
			Push(new IfModule(_index, EatToBracket()));
		}
		else if (Eat("[endif]"))
		{
			if (!TryPopIf())
			{
				Abort("[endif] missing corresponding [if:]", _index);
			}
		}
		else if (Eat('['))
		{
			var start = _index;
			var content = EatToBracket();
			AddNonChild(Builtins.MakeBracketed(start, _index, content));
		}
		else if (In("dt") && Eat(':'))
		{
			Pop("dt");
			Push("dd");
		}
		else if (In("tr") && (tmp = EatPipes()) > 0)
		{
			TryPop("td");
			TryPop("th");
			if (EatWhitespaceOnlyToEolEof())
			{
				SwitchToLine();
			}
			else
			{
				Push(tmp == 1 ? "td" : "th");
			}
		}
		else if (EatEol())
		{
			AddText('\n');
			SwitchToLine();
		}
		else if (Eat('!'))
		{
			if ((url = Eat(Url)) != null)
			{
				AddText(url);
			}
			else
			{
				AddText('!');
			}
		}
		else if ((url = Eat(Url)) != null)
		{
			AddNonChild(Builtins.MakeLink(_index - url.Length, _index, url, new Text(_index - url.Length, url) { CharEnd = _index }));
		}
		else
		{
			AddText(Eat());
		}
	}

	private string ComputeExistingBullets()
	{
		var ret = new StringBuilder();
		foreach (var e in _stack)
		{
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
			{
				break;
			}
		}

		for (var i = old.Length - 1; i >= keep; i--)
		{
			Pop(old[i] == '*' ? "ul" : "ol");
		}

		for (var i = keep; i < nue.Length; i++)
		{
			if (i == 0)
			{
				ClearBlockTags();
			}
			else if (!In("li"))
			{
				Push("li");
			}

			Push(nue[i] == '*' ? "ul" : "ol");
			Push("li");
		}

		if (nue.Length > 0)
		{
			if (keep == nue.Length)
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
		int tmp;

		if (StartLineLists())
		{
			SwitchToInline();
		}
		else if (Eat("%%QUOTE_END"))
		{
			if (!TryPop("figure"))
			{
				Abort("Mismatched %%QUOTE_END", _index);
			}
		}
		else if (Eat("%%QUOTE"))
		{
			var authorBlock = new Element(_index, "figcaption");
			authorBlock.Attributes["class"] = "author";
			var author = EatClassText();

			ClearBlockTags();

			var e = new Element(_index, "figure");
			if (author != "")
			{
				authorBlock.Children.Add(new Text(authorBlock.CharStart, "Quoting " + author));
				e.Children.Add(authorBlock);
			}

			Push(e);
		}
		else if (Eat("%%DIV_END"))
		{
			if (!TryPop("div"))
			{
				Abort("Mismatched %%DIV_END", _index);
			}
		}
		else if (Eat("%%DIV"))
		{
			var className = EatClassText();
			ClearBlockTags();
			var e = new Element(_index, "div");
			if (className != "")
			{
				e.Attributes["class"] = className;
			}

			Push(e);
		}
		else if (Eat("%%TAB "))
		{
			var name = EatTabName();
			ClearBlockTags();
			if (!In("vtabs") && !In("htabs"))
			{
				Push("vtabs");
			}
			else
			{
				TryPopTab();
			}

			var e = new Element(_index, "tab") { Attributes = { ["data-name"] = name } };
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
			{
				Abort("Mismatched %%TAB_END%%", _index);
			}
		}
		else if (Eat("[if:"))
		{
			ClearBlockTags();
			Push(new IfModule(_index, EatToBracket()));
		}
		else if (Eat("[endif]"))
		{
			if (!TryPopIf())
			{
				Abort("[endif] missing corresponding [if:]", _index);
			}
		}
		else if (Eat("----"))
		{
			while (Eat('-'))
			{
			}

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
		else if ((tmp = EatPipes()) > 0)
		{
			if (!In("table"))
			{
				ClearBlockTags();
				Push("table");
				Push("tbody");
			}

			TryPop("tr");
			Push("tr");
			Push(tmp == 1 ? "td" : "th");
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
			{
				e.Attributes["class"] = "language-" + lang;
			}

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
		else if (EatEol())
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
		while (!Eof())
		{
			if (_parsingInline)
			{
				ParseInlineText();
			}
			else
			{
				ParseStartLine();
			}
		}

		FinishText();
	}

	private static void ReplaceTabs(IList<INode> n)
	{
		NodeUtils.Replace(
			n,
			e => e.Type == NodeType.Element && ((Element)e).Tag is "htabs" or "vtabs",
			e => Builtins.MakeTabs((Element)e));
	}

	private static void ReplaceTocs(List<INode> n)
	{
		NodeUtils.Replace(
			n,
			e => e.Type == NodeType.Element && ((Element)e).Tag == "toc",
			e => Builtins.MakeToc(n, e.CharStart));
	}

	private static readonly Regex AllowedIdChars = new("[^a-zA-Z0-9 ]+");

	private static void AddIdsToHeadings(IEnumerable<INode> n)
	{
		static IEnumerable<string> GeneratePossibleIds(IList<string> words)
		{
			if (words.Count > 0 && (words[0] == "The" || words[0] == "A"))
			{
				yield return string.Join("", words.Skip(1));
			}

			yield return string.Join("", words);
			for (var i = 2; ; i++)
			{
				yield return string.Join("", words) + "_" + i;
			}
		}

		var headings = NodeUtils.Find(
				n,
				e => e.Type == NodeType.Element && Builtins.TocHeadings.Contains(((Element)e).Tag))
			.Cast<Element>();
		var ids = new HashSet<string>();

		foreach (var h in headings)
		{
			var originalText = h.InnerText(NullWriterHelper.Instance);
			var filteredText = AllowedIdChars.Replace(originalText, " ");
			var splitByWord = filteredText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var titleCased = splitByWord.Select(s => s.Length > 0 ? char.ToUpperInvariant(s[0]) + s.ToLowerInvariant()[1..] : "");

			var id = GeneratePossibleIds(titleCased.ToList()).First(s => !ids.Contains(s));
			ids.Add(id);
			h.Attributes.Add("id", id);
		}
	}

	private static void ReplacePees(IList<INode> n)
	{
		NodeUtils.Replace(
			n,
			e => e.Type == NodeType.Element && ((Element)e).Tag == "p",
			e =>
			{
				var p = (Element)e;
				var ret = new Element(p.CharStart, "div", p.Children)
				{
					Attributes = { ["class"] = "p" }
				};
				return ret;
			});
	}

	public static List<INode> Parse(string content)
	{
		var p = new NewParser(content);
		p.ParseLoop();
		ReplaceTabs(p._output);
		AddIdsToHeadings(p._output);
		ReplaceTocs(p._output);
		ReplacePees(p._output);
		return p._output;
	}
}
