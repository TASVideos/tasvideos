namespace TASVideos.MovieParsers.Parsers;

[FileExtension("tasproj")]
internal class Tasproj : Bk2
{
	public override string FileExtension => "tasproj";

	protected override string[] InvalidArchiveEntries => new[]
	{
		"greenzone"
	};
}
