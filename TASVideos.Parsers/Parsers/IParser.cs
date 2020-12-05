using System.IO;
using System.Threading.Tasks;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	internal interface IParser
	{
		Task<IParseResult> Parse(Stream file);
	}
}
