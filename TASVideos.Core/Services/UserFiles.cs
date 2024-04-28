using TASVideos.Core.Services.Wiki;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Core.Services;

public interface IUserFiles
{
	/// <summary>
	/// Gets the total user file storage for a given user
	/// </summary>
	Task<int> StorageUsed(int userId);

	/// <summary>
	/// Calculates whether the given file will exceed the maximum limit for the user
	/// </summary>
	Task<bool> SpaceAvailable(int userId, long fileLength);

	/// <summary>
	/// Returns a collection of file extensions that can be uploaded to user files
	/// </summary>
	Task<IReadOnlyCollection<string>> SupportedFileExtensions();

	/// <summary>
	/// Uploads a file for the user to user files
	/// </summary>
	/// <returns>The id of the saved file and an <seealso cref="IParseResult"/> if the file is a movie type</returns>
	Task<(long? Id, IParseResult? ParseResult)> Upload(int userId, UserFileUpload file);
}

internal class UserFiles(
	ApplicationDbContext db,
	IMovieParser parser,
	IFileService fileService,
	IWikiPages wikiPages)
	: IUserFiles
{
	private const string SupplementalUserFileExtensionsPage = "SupplementalUserFileExtensions";

	public async Task<int> StorageUsed(int userId)
	{
		return await db.UserFiles
			.Where(uf => uf.AuthorId == userId)
			.SumAsync(uf => uf.Content.Length);
	}

	public async Task<bool> SpaceAvailable(int userId, long fileLength)
	{
		var storageUsed = await StorageUsed(userId);

		// We calculate storage used by the compressed size in the upload.  This is probably going to be
		// about the same size as what we re-compress it to.
		return storageUsed + fileLength <= SiteGlobalConstants.UserFileStorageLimit;
	}

	public async Task<IReadOnlyCollection<string>> SupportedFileExtensions()
	{
		return parser.SupportedMovieExtensions
			.Concat(await SupportedSupplementalFiles())
			.ToList();
	}

	public async Task<(long? Id, IParseResult? ParseResult)> Upload(int userId, UserFileUpload file)
	{
		var fileExt = Path.GetExtension(file.FileName);
		var userFile = new UserFile
		{
			Id = DateTime.UtcNow.Ticks,
			Title = file.Title,
			Description = file.Description,
			SystemId = file.SystemId,
			GameId = file.GameId,
			AuthorId = userId,
			LogicalLength = file.FileData.Length,
			UploadTimestamp = DateTime.UtcNow,
			Class = (await SupportedSupplementalFiles()).Contains(fileExt)
				? UserFileClass.Support
				: UserFileClass.Movie,
			Type = fileExt.Replace(".", ""),
			FileName = file.FileName,
			Hidden = file.Hidden
		};

		IParseResult? parseResult = null;
		if (parser.SupportedMovieExtensions.Contains(fileExt))
		{
			parseResult = await parser.ParseFile(file.FileName, new MemoryStream(file.FileData, false));
			if (parseResult.Errors.Any())
			{
				return (null, parseResult);
			}

			userFile.Rerecords = parseResult.RerecordCount;
			userFile.Frames = parseResult.Frames;
			if (!string.IsNullOrWhiteSpace(parseResult.Annotations))
			{
				userFile.Annotations = parseResult.Annotations.CapAndEllipse(3500);
			}

			decimal frameRate = 60.0M;
			if (parseResult.FrameRateOverride.HasValue)
			{
				frameRate = (decimal)parseResult.FrameRateOverride.Value;
			}
			else
			{
				var system = await db.GameSystems.SingleOrDefaultAsync(s => s.Code == parseResult.SystemCode);
				if (system != null)
				{
					var frameRateData = await db.GameSystemFrameRates
						.ForSystem(system.Id)
						.ForRegion(parseResult.Region.ToString())
						.FirstOrDefaultAsync();

					if (frameRateData is not null)
					{
						frameRate = (decimal)frameRateData.FrameRate;
					}
				}
			}

			userFile.Length = userFile.Frames / frameRate;
		}

		var fileResult = await fileService.Compress(file.FileData);

		userFile.PhysicalLength = fileResult.CompressedSize;
		userFile.CompressionType = fileResult.Type;
		userFile.Content = fileResult.Data;

		db.UserFiles.Add(userFile);
		await db.SaveChangesAsync();
		return (userFile.Id, parseResult);
	}

	internal async Task<IEnumerable<string>> SupportedSupplementalFiles()
	{
		var page = await wikiPages.SystemPage(SupplementalUserFileExtensionsPage);
		if (page is null)
		{
			return [];
		}

		var types = page.Markup.SplitWithEmpty(",").Select(s => s.Trim());
		return types;
	}
}

public record UserFileUpload(
	string Title,
	string Description,
	int? SystemId,
	int? GameId,
	byte[] FileData,
	string FileName,
	bool Hidden);
