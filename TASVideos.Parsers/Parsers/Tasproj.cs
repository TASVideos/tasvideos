namespace TASVideos.MovieParsers.Parsers;

[FileExtension("tasproj")]
internal class Tasproj : Bk2
{
	protected override string[] InvalidArchiveEntries => new[]
	{
		"greenzone"
	};
}
