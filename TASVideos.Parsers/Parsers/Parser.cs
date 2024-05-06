using System.Reflection;

namespace TASVideos.MovieParsers.Parsers;

internal class Parser
{
	protected ErrorResult InvalidFormat() => new($"Invalid file format, does not seem to be a {FileExtension}") { FileExtension = FileExtension };
	protected ErrorResult Error(string errorMsg) => new(errorMsg) { FileExtension = FileExtension };

	protected string FileExtension
		=> GetType().GetCustomAttribute<FileExtensionAttribute>()?.Extension ?? "unknown";
}
