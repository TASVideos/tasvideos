using System.IO;

namespace TASVideos.WikiEngine
{
	public static class Escape
	{
		public static void WriteCSharpString(TextWriter w, string s)
		{
			w.Write('"');
			foreach (var c in s)
			{
				if (c < 0x20)
				{
					w.Write($"\\x{(int)c:x2}");
				}
				else if (c == '"')
				{
					w.Write("\\\"");
				}
				else
				{
					w.Write(c);
				}
			}

			w.Write('"');
		}
	}
}
