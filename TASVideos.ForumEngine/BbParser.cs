using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TASVideos.ForumEngine
{
	public class BbParser
	{
		private static readonly Regex OpeningTag = new Regex(@"\G([^\p{C}\[\]=\/]+)(=([^\p{C}\[\]=]+))?\]");
		private static readonly Regex ClosingTag = new Regex(@"\G\/([^\p{C}\[\]=\/]+)\]");

		private static readonly HashSet<string> KnownTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"quote", "video", "code", "b", "i", "img", "size", "url", "post", "thread", "movie", "userfile", "u",
			"sup", "sub", "submission", "list",
			"spoiler", "wiki", "s", "frames", "tt", "color",
			// TODO: special processing
			"*", // list item, has no closer
			"noparse" // verbatim text inside
		};

		public static Element Parse(string text)
		{
			var p = new BbParser { _input = text };
			p.ParseLoop();
			return p._root;
		}

		private Element _root = new Element { Name = "_root" };
		private Stack<Element> _stack = new Stack<Element>();

		private string _input;
		private int _index = 0;

		private StringBuilder _currentText = new StringBuilder();

		private BbParser()
		{
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

		private void ParseLoop()
		{
			while (_index < _input.Length)
			{
				var c = _input[_index++];
				if (c == '[') // check for possible tags
				{
					Match m;
					if ((m = OpeningTag.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value;
						var options = m.Groups[3].Value;
						int i;
						if ((i = name.IndexOf(':')) > 0)
						{
							// TODO: wtf is this
							name = name.Substring(0, i);
						}
						if (KnownTags.Contains(name))
						{
							FlushText();
							var e = new Element { Name = name, Options = options };
							_stack.Peek().Children.Add(e);
							_stack.Push(e);
							_index += m.Length;
							continue;
						}
						else
						{
							// TODO: some warning
							Console.WriteLine("##### Uknown tag " + name + " ##############################");
							if (name == "0xED" || name == "color")
							{
								Console.WriteLine(_input);
							}
						}
					}
					else if ((m = ClosingTag.Match(_input, _index)).Success)
					{
						var name = m.Groups[1].Value;
						int i;
						if ((i = name.IndexOf(':')) > 0)
						{
							// TODO: wtf is this
							name = name.Substring(0, i);
						}
						if (_stack.Peek().Name == name)
						{
							FlushText();
							_stack.Pop();
							_index += m.Length;
							continue;
						}
						else
						{
							// TODO: some warning
						}
					}
					else
					{
						Console.WriteLine("nomatch!");
					}
				}
				_currentText.Append(c);
			}
			FlushText();
		}
	}
}
