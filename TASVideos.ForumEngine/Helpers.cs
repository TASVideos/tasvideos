using System.IO;

namespace TASVideos.ForumEngine
{
	internal static class Helpers
	{
		public static void WriteText(TextWriter w, string s)
		{
			foreach (var c in s)
			{
				switch (c)
				{
					case '<':
						w.Write("&lt;");
						break;
					case '&':
						w.Write("&amp;");
						break;
					default:
						w.Write(c);
						break;
				}
			}
		}

		public static void WriteAttributeValue(TextWriter w, string s)
		{
			w.Write('"');
			foreach (var c in s)
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
					default:
						w.Write(c);
						break;
				}
			}

			w.Write('"');
		}
	}
}
