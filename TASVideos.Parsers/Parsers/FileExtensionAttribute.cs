namespace TASVideos.MovieParsers.Parsers;

/// <summary>
/// Decorates an <see cref="IParser" /> implementation to
/// indicate which file extension it is capable of parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal class FileExtensionAttribute(string extension) : Attribute
{
	public string Extension { get; } = extension;
}
