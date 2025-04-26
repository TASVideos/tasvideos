using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions;

public class SubmitPageModelBase(IMovieParser parser, IFileService fileService) : BasePageModel
{
	public async Task<(IParseResult ParseResult, byte[] MovieFileBytes)> ParseMovieFile(IFormFile movieFile)
	{
		var fileStream = await movieFile.DecompressOrTakeRaw();
		byte[] fileBytes = fileStream.ToArray();

		var parseResult = movieFile.IsZip()
			? await parser.ParseZip(fileStream)
			: await parser.ParseFile(movieFile.FileName, fileStream);

		byte[] movieFileBytes = movieFile.IsZip()
			? fileBytes
			: await fileService.ZipFile(fileBytes, movieFile.FileName);

		return (parseResult, movieFileBytes);
	}
}
