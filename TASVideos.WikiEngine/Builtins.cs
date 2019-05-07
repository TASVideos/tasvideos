using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TASVideos.WikiEngine.AST;

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
