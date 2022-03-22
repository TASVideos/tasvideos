using System.Reflection;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers;

/// <summary>
/// The entry point for movie file parsers
/// Takes a stream of the zip file containing a movie file
/// The file must have precisely one file
/// The file is processed and a <see cref="IParseResult"/>
/// is returned.
/// </summary>
/// <seealso cref="IParseResult"/>
public interface IMovieParser
{
	IEnumerable<string> SupportedMovieExtensions { get; }
	Task<IParseResult> ParseZip(Stream stream);
	Task<IParseResult> ParseFile(string fileName, Stream stream);
}

public sealed class MovieParser : IMovieParser
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

	public async Task<IParseResult> ParseZip(Stream stream)
	{
		try
		{
			using var zip = new ZipArchive(stream);
			if (zip.Entries.Count > 1)
			{
				return Error("Multiple files detected in the .zip, only one file is allowed");
			}

			var movieFile = zip.Entries[0];
			var ext = Path.GetExtension(movieFile.Name).Trim('.').ToLower();

			var parser = GetParser(ext);
			if (parser == null)
			{
				return Error($".{ext} files are not currently supported.");
			}

			await using var movieFileStream = movieFile.Open();
			return await parser.Parse(movieFileStream, movieFile.Length);
		}
		catch (Exception)
		{
			// TODO: do we want to log here? or catch at a higher layer?
			return Error("An general error occured while processing the movie file.");
		}
	}

	public async Task<IParseResult> ParseFile(string fileName, Stream stream)
	{
		try
		{
			var ext = Path.GetExtension(fileName).Trim('.').ToLower();

			var parser = GetParser(ext);
			return parser == null
				? Error($".{ext} files are not currently supported.")
				: await parser.Parse(stream, stream.Length);
		}
		catch (Exception)
		{
			// TODO: do we want to log here? or catch at a higher layer?
			return Error("An general error occured while processing the movie file.");
		}
	}

	private static IParser? GetParser(string? ext)
	{
		var type = ParserTypes
			.SingleOrDefault(t => (t.GetCustomAttribute(typeof(FileExtensionAttribute)) as FileExtensionAttribute)
				?.Extension == ext);

		if (type == null)
		{
			return null;
		}

		return Activator.CreateInstance(type) as IParser;
	}

	private static IParseResult Error(string errorMsg) => new ErrorResult(errorMsg);
}
