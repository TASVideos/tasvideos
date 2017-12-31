using System.IO;

namespace TASVideos.MovieParsers
{
	internal interface IParser
	{
		IParseResult Parse(Stream file);
	}
}
