using System.IO;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	internal interface IParser
	{
		IParseResult Parse(Stream file);
	}
}
