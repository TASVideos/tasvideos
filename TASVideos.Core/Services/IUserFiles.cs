using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
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
	/// Calculates whether or not the given file will exceed the maximum limit for the user
	/// </summary>
	Task<bool> SpaceAvailable(int userId, long fileLength);

	/// <summary>
	/// Returns a value indicating whether or not the given file extension can be uploaded to user files
	/// </summary>
	Task<bool> IsSupportedFileExtension(string fileExtension);

	/// <summary>
	/// Uploads a file for the user to user files
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown if space is not available <seealso cref="SpaceAvailable"/></exception>
	/// <returns>The id of the saved file and an <seealso cref="IParseResult"/> if the file is a movie type</returns>
	Task<(long id, IParseResult? parseResult)> Upload(int userId, UserFileUpload file);
}

internal class UserFiles : IUserFiles
{
	private const string SupplementalUserFileExtensionsPage = "SupplementalUserFileExtensions";
	private readonly ApplicationDbContext _db;
	private readonly IMovieParser _parser;
	private readonly IFileService _fileService;
	private readonly IWikiPages _wikiPages;

	public UserFiles(
		ApplicationDbContext db,
		IMovieParser parser,
		IFileService fileService,
		IWikiPages wikiPages)
	{
		_db = db;
		_parser = parser;
		_fileService = fileService;
		_wikiPages = wikiPages;
	}

	public async Task<int> StorageUsed(int userId)
	{
		return await _db.UserFiles
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

	public async Task<bool> IsSupportedFileExtension(string fileExtension)
	{
		return (await SupportedSupplementalFiles()).Contains(fileExtension)
			|| _parser.SupportedMovieExtensions.Contains(fileExtension);
	}

	public async Task<(long, IParseResult?)> Upload(int userId, UserFileUpload file)
	{
		if (!await SpaceAvailable(userId, file.FileData.Length))
		{
			throw new InvalidOperationException($"Not enough space available to upload file {file.FileName}");
		}

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
		if (_parser.SupportedMovieExtensions.Contains(fileExt))
		{
			parseResult = await _parser.ParseFile(file.FileName, new MemoryStream(file.FileData, false));
			userFile.Rerecords = parseResult.RerecordCount;
			userFile.Frames = parseResult.Frames;

			decimal frameRate = 60.0M;
			if (parseResult.FrameRateOverride.HasValue)
			{
				frameRate = (decimal)parseResult.FrameRateOverride.Value;
			}
			else
			{
				var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Code == parseResult.SystemCode);
				if (system != null)
				{
					var frameRateData = await _db.GameSystemFrameRates
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

		var fileResult = await _fileService.Compress(file.FileData);

		userFile.PhysicalLength = fileResult.CompressedSize;
		userFile.CompressionType = fileResult.Type;
		userFile.Content = fileResult.Data;

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();
		return (userFile.Id, parseResult);
	}

	internal async Task<IEnumerable<string>> SupportedSupplementalFiles()
	{
		var page = await _wikiPages.SystemPage(SupplementalUserFileExtensionsPage);
		if (page is null)
		{
			return Enumerable.Empty<string>();
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