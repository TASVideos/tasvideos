using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions;

public class SubmitPageModelBase(IMovieParser parser, IFileService fileService) : BasePageModel
{
	protected readonly IFileService fileService = fileService;

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

	public bool CanEditSubmission(string? submitter, ICollection<string> authors)
	{
		// If the user cannot edit submissions, then they must be an author or the original submitter
		if (User.Has(PermissionTo.EditSubmissions))
		{
			return true;
		}

		var user = User.Name();
		var isAuthorOrSubmitter = !string.IsNullOrEmpty(user)
			&& (submitter == user || authors.Contains(user));

		return isAuthorOrSubmitter && User.Has(PermissionTo.SubmitMovies);
	}
}
