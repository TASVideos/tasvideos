using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TASVideos.ForumEngine
{
	public class BbParser
	{
		private static readonly Regex OpeningTag = new Regex(@"\G([^\p{C}\[\]=\/]+)(=([^\p{C}\[\]]+))?\]");
		private static readonly Regex ClosingTag = new Regex(@"\G\/([^\p{C}\[\]=\/]+)\]");
		private static readonly Regex Url = new Regex(@"\Ghttps?:\/\/([A-Za-z0-9\-._~!$&'()*+,;=:@\/]|%[A-Fa-f0-9]{2})+");

		private enum ParseState
		{
			ChildContent,
			ChildContentIfParam,
			NoChildContent,
			List,
			ListItem
		}

		private static readonly Dictionary<string, ParseState> KnownTags = new Dictionary<string, ParseState>
		{
			// basic text formatting, no params, and body is content
			{ "b", ParseState.ChildContent },
			{ "i", ParseState.ChildContent },
			{ "u", ParseState.ChildContent },
			{ "s", ParseState.ChildContent },
			{ "sub", ParseState.ChildContent },
			{ "sup", ParseState.ChildContent },
			{ "tt", ParseState.ChildContent },
			{ "left", ParseState.ChildContent },
			{ "right", ParseState.ChildContent },
			{ "center", ParseState.ChildContent },
			{ "spoiler", ParseState.ChildContent },

			// with optional params
			{ "quote", ParseState.ChildContent }, // optional author
			{ "code", ParseState.NoChildContent }, // optional language
			{ "img", ParseState.NoChildContent }, // optional size
			{ "url", ParseState.ChildContentIfParam }, // optional url.  if not given, url in body
			{ "email", ParseState.ChildContentIfParam }, // like url
			{ "video", ParseState.NoChildContent }, // like img
			{ "google", ParseState.NoChildContent }, // search query in body.  optional param `images`
			{ "thread", ParseState.ChildContentIfParam }, // like url, but the link is a number
			{ "post", ParseState.ChildContentIfParam }, // like thread
			{ "movie", ParseState.ChildContentIfParam }, // like thread
			{ "submission", ParseState.ChildContentIfParam }, // like thread
			{ "userfile", ParseState.ChildContentIfParam }, // like thread
			{ "wiki", ParseState.ChildContentIfParam }, // like thread, but the link is a page name

			// other stuff
			{ "frames", ParseState.NoChildContent }, // no params.  body is something like `200` or `200@60.1`
			{ "color", ParseState.ChildContent }, // param is a css (?) color
			{ "size", ParseState.ChildContent }, // param is something relating to font size TODO: what are the values?
			{ "noparse", ParseState.NoChildContent },

			// list related stuff
			{ "list", ParseState.List }, // OLs have a param with value ??
			{ "*", ParseState.ListItem },
		};

		public static Element Parse(string text)
		{
			var p = new BbParser(text);
			p.ParseLoop();
			return p._root;
		}

		private readonly Element _root = new Element { Name = "_root" };
		private readonly Stack<Element> _stack = new Stack<Element>();

		private readonly string _input;
		private int _index = 0;

		private readonly StringBuilder _currentText = new StringBuilder();

		private BbParser(string input)
		{
			_input = input;
			_stack.Push(_root);
		}

		private void FlushText()
		{
			if (_currentText.Length > 0)
			{
				_stack.Peek().Children.Add(new Text { Content = _currentText.ToString() });
				_currentText.Clear();
			}
		}

		private void Push(Element e)
		{
			_stack.Peek().Children.Add(e);
			_stack.Push(e);
		}

		private bool ChildrenExpected()
		{
			if (KnownTags.TryGetValue(_stack.Peek().Name, out var state))
			{
				switch (state)
				{
					case ParseState.NoChildContent:
						return false;
					case ParseState.ChildContent:
					case ParseState.List:
					case ParseState.ListItem:
					default:
						return true;
					case ParseState.ChildContentIfParam:
						return _stack.Peek().Options != "";
				}
			}

			// "li" or "_root"
			return true;
		}

		private void ParseLoop()
		{
			while (_index < _input.Length)
			{
				Match urlMatch;
				if (ChildrenExpected() && (urlMatch = Url.Match(_input, _index)).Success)
				{
					FlushText();
					Push(new Element { Name = "url" });
					_currentText.Append(urlMatch.Value);
					FlushText();
					_index += urlMatch.Length;
					_stack.Pop();
					continue;
				}

				var c = _input[_index++];
				if (c == '[') // check for possible tags
				{
					Match m;
					if (ChildrenExpected() && (m = OpeningTag.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value;
						var options = m.Groups[3].Value;
						if (KnownTags.TryGetValue(name, out var state))
						{
							var e = new Element { Name = name, Options = options };
							if (state == ParseState.List)
							{
								FlushText();
								_index += m.Length;
								Push(e);
								Push(new Element { Name = "li" });
							}
							else if (state == ParseState.ListItem)
							{
								// try to pop a list item, then push a new one
								if (_stack.Peek().Name == "li")
								{
									FlushText();
									_index += m.Length;
									_stack.Pop();
									Push(new Element { Name = "li" });
								}
								else
								{
									_currentText.Append(c);
								}
							}
							else
							{
								FlushText();
								_index += m.Length;
								Push(e);
							}

							continue;
						}
						else
						{
							// Tag not recognized?  OK, process as raw text
						}
					}
					else if ((m = ClosingTag.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value;
						var topName = _stack.Peek().Name;
						if (topName == name)
						{
							FlushText();
							_index += m.Length;
							_stack.Pop();
							continue;
						}
						else if (topName == "li" && name == "list")
						{
							// pop a list
							FlushText();
							_index += m.Length;
							_stack.Pop();
							_stack.Pop();
							continue;
						}
						else
						{
							// closing didn't match opening?  OK, process as raw text
						}
					}
					else
					{
						// '[' but not followed by a valid tag?  OK, process as raw text
					}
				}

				_currentText.Append(c);
			}

			FlushText();
		}
	}
}
