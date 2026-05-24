using System.Reflection;
using TASVideos.MovieParsers.Parsers;

namespace TASVideos.MovieParsers;

/// <summary>
/// The entry point for movie file parsers
/// Takes a stream of the movie file
/// The file is processed and an <see cref="IParseResult"/>
/// is returned.
/// </summary>
/// <seealso cref="IParseResult"/>
public interface IMovieParser
{
	IEnumerable<string> SupportedMovieExtensions { get; }
	Task<IParseResult> ParseFile(string fileName, Stream stream);
}

internal sealed class MovieParser : IMovieParser
{
	private static readonly ICollection<Type> ParserTypes =
		typeof(IParser).Assembly
			.GetTypes()
			.Where(t => typeof(IParser).IsAssignableFrom(t))
			.Where(t => t != typeof(IParser))
			.Where(t => t.GetCustomAttributes().OfType<FileExtensionAttribute>().Any())
			.ToList();

	public IEnumerable<string> SupportedMovieExtensions => ParserTypes
		.Select(t => "." + (t.GetCustomAttribute(typeof(FileExtensionAttribute)) as FileExtensionAttribute)
				?.Extension);

	public async Task<IParseResult> ParseFile(string fileName, Stream stream)
	{
		try
		{
			var ext = Path.GetExtension(fileName).Trim('.').ToLower();

			var parser = GetParser(ext);
			return parser is null
				? Error($".{ext} files are not currently supported.")
				: await parser.Parse(stream, stream.Length);
		}
		catch (Exception)
		{
			// TODO: do we want to log here? or catch at a higher layer?
			return Error("A general error occured while processing the movie file.");
		}
	}

	private static IParser? GetParser(string? ext)
	{
		var type = ParserTypes
			.SingleOrDefault(t => (t.GetCustomAttribute(typeof(FileExtensionAttribute)) as FileExtensionAttribute)
				?.Extension == ext);

		if (type is null)
		{
			return null;
		}

		return Activator.CreateInstance(type) as IParser;
	}

	private static ErrorResult Error(string errorMsg) => new(errorMsg);
}
