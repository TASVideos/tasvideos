using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

internal abstract class ParserBase
{
	public abstract string FileExtension { get; }

	protected ErrorResult Error(string errorMsg)
	{
		return new(errorMsg) { FileExtension = FileExtension };
	}
}
