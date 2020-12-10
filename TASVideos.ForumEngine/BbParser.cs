using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TASVideos.ForumEngine
{
	public class BbParser
	{
		private static readonly Regex OpeningTag = new(@"\G([^\p{C}\[\]=\/]+)(=([^\p{C}\[\]]+))?\]");
		private static readonly Regex ClosingTag = new(@"\G\/([^\p{C}\[\]=\/]+)\]");
		private static readonly Regex Url = new(@"\Ghttps?:\/\/([A-Za-z0-9\-._~!$&'()*+,;=:@\/]|%[A-Fa-f0-9]{2})+");

		// The old system does support attributes in html tags, but only a few that we probably don't want,
		// and it doesn't even support the full html syntax for them.  So forget attributes for now
		private static readonly Regex HtmlOpening = new(@"\G\s*([a-zA-Z]+)\s*>");
		private static readonly Regex HtmlClosing = new(@"\G\s*\/\s*([a-zA-Z]+)\s*>");

		private static readonly Regex HtmlVoid = new(@"\G\s*([a-zA-Z]+)\s*\/?\s*>");

		/// <summary>
		/// what content is legal at this time
		/// </summary>
		private enum ParseState
		{
			/// <summary>
			/// text and bbcode tags are legal
			/// </summary>
			/// <value></value>
			ChildTags,
			/// <summary>
			/// if the parent bbcode tag has a parameter, text and bbcode tags are legal.  otherwise, raw text only
			/// </summary>
			/// <value></value>
			ChildTagsIfParam,
			/// <summary>
			/// everything except a matching bbcode end tag is raw text
			/// </summary>
			/// <value></value>
			NoChildTags,
			/// <summary>
			/// Like ChildTags, but this element cannot nest itself
			/// </summary>
			/// <value></value>
			ChildTagsNoNest,
			/// <summary>
			/// Like ChildTags, but this element cannot nest directly in itself
			/// </summary>
			/// <value></value>
			ChildTagsNoImmediateNest
		}

		private static readonly Dictionary<string, ParseState> KnownTags = new()
		{
			// basic text formatting, no params, and body is content
			{ "b", ParseState.ChildTags },
			{ "i", ParseState.ChildTags },
			{ "u", ParseState.ChildTags },
			{ "s", ParseState.ChildTags },
			{ "sub", ParseState.ChildTags },
			{ "sup", ParseState.ChildTags },
			{ "tt", ParseState.ChildTags },
			{ "left", ParseState.ChildTags },
			{ "right", ParseState.ChildTags },
			{ "center", ParseState.ChildTags },
			{ "spoiler", ParseState.ChildTags },
			{ "warning", ParseState.ChildTags },
			{ "note", ParseState.ChildTags },
			{ "highlight", ParseState.ChildTags },

			// with optional params
			{ "quote", ParseState.ChildTags }, // optional author
			{ "code", ParseState.NoChildTags }, // optional language
			{ "img", ParseState.NoChildTags }, // optional size
			{ "url", ParseState.ChildTagsIfParam }, // optional url.  if not given, url in body
			{ "email", ParseState.ChildTagsIfParam }, // like url
			{ "video", ParseState.NoChildTags }, // like img
			{ "google", ParseState.NoChildTags }, // search query in body.  optional param `images`
			{ "thread", ParseState.ChildTagsIfParam }, // like url, but the link is a number
			{ "post", ParseState.ChildTagsIfParam }, // like thread
			{ "movie", ParseState.ChildTagsIfParam }, // like thread
			{ "submission", ParseState.ChildTagsIfParam }, // like thread
			{ "userfile", ParseState.ChildTagsIfParam }, // like thread
			{ "wip", ParseState.ChildTagsIfParam }, // like thread (in fact, identical to userfile except for text output)
			{ "wiki", ParseState.ChildTagsIfParam }, // like thread, but the link is a page name

			// other stuff
			{ "frames", ParseState.NoChildTags }, // no params.  body is something like `200` or `200@60.1`
			{ "color", ParseState.ChildTags }, // param is a css (?) color
			{ "bgcolor", ParseState.ChildTags }, // like color
			{ "size", ParseState.ChildTags }, // param is something relating to font size TODO: what are the values?
			{ "noparse", ParseState.NoChildTags },

			// list related stuff
			{ "list", ParseState.ChildTags }, // OLs have a param with value ??
			{ "*", ParseState.ChildTagsNoImmediateNest },

			// tables
			{ "table", ParseState.ChildTagsNoNest },
			{ "tr", ParseState.ChildTagsNoNest },
			{ "td", ParseState.ChildTagsNoNest }
		};

		private static readonly HashSet<string> KnownNonEmptyHtmlTags = new()
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
			"small"
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

		private readonly Element _root = new() { Name = "_root" };
		private readonly Stack<Element> _stack = new();

		private readonly string _input;
		private int _index;

		private readonly bool _allowHtml;
		private readonly bool _allowBb;
		private bool _didHtml;

		private readonly StringBuilder _currentText = new();

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
				return state switch
				{
					ParseState.NoChildTags => false,
					ParseState.ChildTagsIfParam => _stack.Peek().Options != "",
					_ => true,
				};
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
							FlushText();
							_index += m.Length;
							if (state == ParseState.ChildTagsNoNest)
							{
								// try to pop a matching tag
								foreach (var node in _stack)
								{
									if (node.Name == name)
									{
										while (true)
										{
											if (_stack.Pop().Name == name)
												break;
										}
										break;
									}
								}
							}
							else if (state == ParseState.ChildTagsNoImmediateNest)
							{
								// try to pop a matching tag but only at this level
								if (_stack.Peek().Name == name)
									_stack.Pop();
							}
							Push(e);
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

						if (topName == "*" && name == "list")
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
					if ((m = HtmlClosing.Match(_input, _index)).Success)
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
					else if (ChildrenExpected())
					{
						if ((m = HtmlOpening.Match(_input, _index)).Success)
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
								// tag not recognized?  Might be a void tag, or raw text
							}
						}
						if ((m = HtmlVoid.Match(_input, _index)).Success)
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
