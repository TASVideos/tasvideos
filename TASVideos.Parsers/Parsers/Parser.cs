using System.Reflection;

namespace TASVideos.MovieParsers.Parsers;

internal class Parser
{
	protected ErrorResult Error(string errorMsg) => new(errorMsg) { FileExtension = FileExtension };

	protected string FileExtension
		=> GetType().GetCustomAttribute<FileExtensionAttribute>()?.Extension ?? "unknown";
}
