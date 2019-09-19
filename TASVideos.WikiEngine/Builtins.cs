using System.Collections.Generic;

namespace TASVideos.WikiEngine
{
	public static partial class Builtins
	{
		private static KeyValuePair<string, string> Attr(string name, string value)
		{
			return new KeyValuePair<string, string>(name, value);
		}
	}
}
