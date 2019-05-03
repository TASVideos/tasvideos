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

		// The old system does suport attributes in html tags, but only a few that we probably don't want,
		// and it doesn't even support the full html syntax for them.  So forget attributes for now
		private static readonly Regex HtmlOpening = new Regex(@"\G\s*([a-zA-Z]+)\s*>");
		private static readonly Regex HtmlClosing = new Regex(@"\G\s*\/\s*([a-zA-Z]+)\s*>");

		private static readonly Regex HtmlVoid = new Regex(@"\G\s*([a-zA-Z]+)\s*\/?\s*>");

		/// <summary>
		/// what content is legal at this time
		/// </summary>
		private enum ParseState
		{
			/// <summary>
			/// text and bbcode tags are legal
			/// </summary>
			/// <value></value>
			ChildContent,
			/// <summary>
			/// if the parent bbcode tag has a parameter, text and bbcode tags are legal.  otherwise, raw text only
			/// </summary>
			/// <value></value>
			ChildContentIfParam,
			/// <summary>
			/// everything except a matching bbcode end tag is raw text
			/// </summary>
			/// <value></value>
			NoChildContent,
			/// <summary>
			/// like ChildContent, but including special handling for listitem tags which are not closed
			/// </summary>
			/// <value></value>
			List,
			/// <summary>
			/// like ChildContent, but including special handling for listitem tags which are not closed
			/// </summary>
			/// <value></value>
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

		private static readonly HashSet<string> KnownNonEmptyHtmlTags = new HashSet<string>
		{
			// html parsing, except the empty tags <br> and <hr>, as they immediately close
			// so their parse state is not needed
			"b",
			"i",
			"em",
			"u",
			"pre",
			"code",
			"tt",
			"strike",
			"s",
			"del",
			"sup",
			"sub",
			"div",
			"small",
		};

		public static Element Parse(string text, bool allowHtml, bool allowBb)
		{
			var p = new BbParser(text, allowHtml, allowBb);
			p.ParseLoop();
			return p._root;
		}
		public static bool ContainsHtml(string text, bool allowBb)
		{
			var p = new BbParser(text, true, allowBb);
			p.ParseLoop();
			return p._didHtml;
		}

		private readonly Element _root = new Element { Name = "_root" };
		private readonly Stack<Element> _stack = new Stack<Element>();

		private readonly string _input;
		private int _index = 0;

		private readonly bool _allowHtml;
		private readonly bool _allowBb;
		private bool _didHtml;

		private readonly StringBuilder _currentText = new StringBuilder();

		private BbParser(string input, bool allowHtml, bool allowBb)
		{
			_input = input;
			_allowHtml = allowHtml;
			_allowBb = allowBb;
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

			// "li" or "_root" or any of the html tags
			return true;
		}

		private void ParseLoop()
		{
			while (_index < _input.Length)
			{
				{
					Match m;
					if (_allowBb && ChildrenExpected() && (m = Url.Match(_input, _index)).Success)
					{
						FlushText();
						Push(new Element { Name = "url" });
						_currentText.Append(m.Value);
						FlushText();
						_index += m.Length;
						_stack.Pop();
						continue;
					}
				}

				var c = _input[_index++];
				if (_allowBb && c == '[') // check for possible tags
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
				else if (_allowHtml && c == '<') // check for possible HTML tags
				{
					Match m;
					if (ChildrenExpected() && (m = HtmlOpening.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value.ToLowerInvariant();
						if (KnownNonEmptyHtmlTags.Contains(name))
						{
							var e = new Element { Name = "html:" + name };
							FlushText();
							_index += m.Length;
							Push(e);
							_didHtml = true;
							continue;
						}
						else
						{
							// tag not recognized?  OK, process as raw text
						}
					}
					else if (ChildrenExpected() && (m = HtmlVoid.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value.ToLowerInvariant();
						if (name == "br" || name == "hr")
						{
							var e = new Element { Name = "html:" + name };
							FlushText();
							_index += m.Length;
							Push(e);
							_stack.Pop();
							_didHtml = true;
							continue;
						}
						else
						{
							// tag not recognized?  OK, process as raw text
						}
					}
					else if ((m = HtmlClosing.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value.ToLowerInvariant();
						name = "html:" + name;
						var topName = _stack.Peek().Name;
						if (name == topName)
						{
							FlushText();
							_index += m.Length;
							_stack.Pop();
							_didHtml = true;
							continue;
						}
						else
						{
							// closing didn't match opening?  OK, process as raw text
						}
					}
					else
					{
						// '<' but not followed by a valid tag?  OK, process as raw text
					}
				}

				_currentText.Append(c);
			}

			FlushText();
		}
	}
}
