using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Services;

public interface IPublications
{
	/// <summary>
	/// Returns the title of a publication with the given id, or null if the publication is not found
	/// </summary>
	Task<string?> GetTitle(int publicationId);

	/// <summary>
	/// Returns the publication Urls for a given publication, empty list if publication not found, or it has no urls
	/// </summary>
	Task<ICollection<PublicationUrl>> GetUrls(int publicationId);

	/// <summary>
	/// Removes the publication url
	/// </summary>
	/// <returns>Null if url is not found and successfully deleted, else null</returns>
	Task<PublicationUrl?> RemoveUrl(int urlId);

	Task<SaveResult> AddMovieFile(int pubId, string fileName, string description, byte[] file);

	/// <summary>
	/// Removes the given file from the given publication
	/// </summary>
	/// <returns>The deleted file, or null if the file was not found or unable to be removed</returns>
	Task<(PublicationFile? File, SaveResult Result)> RemoveFile(int fileId);

	/// <summary>
	/// Returns whether a publication can be unpublished, does not affect the publication
	/// </summary>
	Task<UnpublishResult> CanUnpublish(int publicationId);

	/// <summary>
	/// Deletes a publication and returns the corresponding submission back to the submission queue
	/// </summary>
	Task<UnpublishResult> Unpublish(int publicationId);

	/// <summary>
	/// Updates a publication's metadata including authors, flags, tags, and other properties
	/// </summary>
	/// <returns>List of change messages for logging/notification purposes</returns>
	Task<UpdatePublicationResult> UpdatePublication(int publicationId, UpdatePublicationRequest request);

	/// <summary>
	/// Returns the available movie files for a given publication
	/// </summary>
	Task<List<FileEntry>> GetAvailableMovieFiles(int publicationId);
}

internal class Publications(
	ApplicationDbContext db,
	IYoutubeSync youtubeSync,
	ITASVideoAgent tva,
	IWikiPages wikiPages,
	ITagService tagService,
	IFlagService flagService,
	IFileService fileService)
	: IPublications
{
	public Task<string?> GetTitle(int publicationId)
		=> db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

	public async Task<ICollection<PublicationUrl>> GetUrls(int publicationId)
		=> await db.PublicationUrls
			.Where(u => u.PublicationId == publicationId)
			.ToListAsync();

	public async Task<SaveResult> AddMovieFile(int pubId, string fileName, string description, byte[] file)
	{
		var compressed = await fileService.Compress(file);

		db.PublicationFiles.Add(new PublicationFile
		{
			PublicationId = pubId,
			Path = fileName,
			Description = description,
			Type = FileType.MovieFile,
			FileData = compressed.Data,
			CompressionType = compressed.Type
		});

		return await db.TrySaveChanges();
	}

	public async Task<PublicationUrl?> RemoveUrl(int urlId)
	{
		var url = await db.PublicationUrls.FindAsync(urlId);
		if (url is null)
		{
			return null;
		}

		db.PublicationUrls.Remove(url);
		var saveResult = await db.TrySaveChanges();
		if (!saveResult.IsSuccess())
		{
			return null;
		}

		return url;
	}

	public async Task<(PublicationFile? File, SaveResult Result)> RemoveFile(int fileId)
	{
		var file = await db.PublicationFiles.FindAsync(fileId);
		if (file is null)
		{
			return (null, SaveResult.NotFound);
		}

		db.PublicationFiles.Remove(file);
		var result = await db.TrySaveChanges();
		return (file, result);
	}

	public async Task<UnpublishResult> CanUnpublish(int publicationId)
	{
		var pub = await db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => new
			{
				p.Title,
				HasAwards = p.PublicationAwards.Any()
			})
			.SingleOrDefaultAsync();

		if (pub is null)
		{
			return UnpublishResult.NotFound();
		}

		return pub.HasAwards
			? UnpublishResult.HasAwards(pub.Title)
			: UnpublishResult.Success(pub.Title);
	}

	public async Task<UnpublishResult> Unpublish(int publicationId)
	{
		var publication = await db.Publications
			.Include(p => p.PublicationAwards)
			.Include(p => p.Authors)
			.Include(p => p.Files)
			.Include(p => p.PublicationFlags)
			.Include(p => p.PublicationRatings)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationUrls)
			.Include(p => p.Submission)
			.Include(p => p.ObsoletedMovies)
			.ThenInclude(o => o.PublicationUrls)
			.SingleOrDefaultAsync(p => p.Id == publicationId);

		if (publication is null)
		{
			return UnpublishResult.NotFound();
		}

		if (publication.PublicationAwards.Any())
		{
			return UnpublishResult.HasAwards(publication.Title);
		}

		var youtubeUrls = publication.PublicationUrls
			.ThatAreStreaming()
			.Select(pu => pu.Url)
			.Where(youtubeSync.IsYoutubeUrl)
			.ToList();

		var obsoletedPubsWithYoutube = publication.ObsoletedMovies
			.Where(p => p.PublicationUrls.Any(u => youtubeSync.IsYoutubeUrl(u.Url)))
			.ToList();

		publication.Authors.Clear();
		publication.Files.Clear();
		publication.PublicationFlags.Clear();
		publication.PublicationRatings.Clear();
		publication.PublicationTags.Clear();
		publication.PublicationUrls.Clear();
		var logs = await db.PublicationMaintenanceLogs
			.Where(l => l.PublicationId == publicationId)
			.ToListAsync();
		db.RemoveRange(logs);

		// Note: Cascading deletes will ensure obsoleted publications are no longer obsoleted
		db.Publications.Remove(publication);
		db.SubmissionStatusHistory.Add(publication.SubmissionId, publication.Submission!.Status);
		publication.Submission.Status = PublicationUnderway;

		await tva.PostSubmissionUnpublished(publication.SubmissionId);
		await db.SaveChangesAsync();
		await wikiPages.Delete(WikiHelper.ToPublicationWikiPageName(publicationId));

		foreach (var url in youtubeUrls)
		{
			await youtubeSync.UnlistVideo(url!);
		}

		foreach (var obsoletedPub in obsoletedPubsWithYoutube)
		{
			// Re-query to get all the includes
			// We can afford these extra trips, compared to the massive query it would be
			// for a single trip
			var queriedPub = await db.Publications
				.Include(p => p.PublicationUrls)
				.Include(p => p.System)
				.Include(p => p.Game)
				.Include(p => p.Authors)
				.ThenInclude(pa => pa.Author)
				.SingleAsync(p => p.Id == obsoletedPub.Id);

			var obsoletedWiki = await wikiPages.PublicationPage(queriedPub.Id);

			foreach (var url in obsoletedPub.PublicationUrls.Where(u => youtubeSync.IsYoutubeUrl(u.Url)))
			{
				await youtubeSync.SyncYouTubeVideo(new YoutubeVideo(
					queriedPub.Id,
					queriedPub.CreateTimestamp,
					url.Url!,
					url.DisplayName,
					queriedPub.Title,
					obsoletedWiki!,
					queriedPub.System!.Code,
					queriedPub.Authors.OrderBy(pa => pa.Ordinal).Select(a => a.Author!.UserName),
					queriedPub.ObsoletedById));
			}
		}

		return UnpublishResult.Success(publication.Title);
	}

	public async Task<UpdatePublicationResult> UpdatePublication(int publicationId, UpdatePublicationRequest request)
	{
		var changeMessages = new List<string>();

		var publication = await db.Publications
			.Include(p => p.Authors)
			.ThenInclude(pa => pa.Author)
			.Include(p => p.System)
			.Include(p => p.SystemFrameRate)
			.Include(p => p.Game)
			.Include(p => p.GameVersion)
			.Include(p => p.GameGoal)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationFlags)
			.Include(p => p.PublicationUrls)
			.SingleOrDefaultAsync(p => p.Id == publicationId);

		if (publication is null)
		{
			return new UpdatePublicationResult(false, []);
		}

		if (publication.ObsoletedById != request.ObsoletedBy)
		{
			changeMessages.Add($"Changed obsoleting movie from \"{publication.ObsoletedById}\" to \"{request.ObsoletedBy}\"");
		}

		if (publication.AdditionalAuthors != request.ExternalAuthors)
		{
			changeMessages.Add($"Changed external authors from \"{publication.AdditionalAuthors}\" to \"{request.ExternalAuthors}\"");
		}

		publication.ObsoletedById = request.ObsoletedBy;
		publication.EmulatorVersion = request.EmulatorVersion;
		publication.AdditionalAuthors = request.ExternalAuthors.NormalizeCsv();

		publication.Authors.Clear();
		publication.Authors.AddRange(await db.Users
			.ForUsers(request.Authors)
			.Select(u => new PublicationAuthor
			{
				PublicationId = publicationId,
				UserId = u.Id,
				Author = u,
				Ordinal = request.Authors.IndexOf(u.UserName)
			})
			.ToListAsync());

		publication.Title = publication.GenerateTitle();

		// Handle flags with permission filtering
		List<int> editableFlags = await db.Flags
			.Where(f => f.PermissionRestriction.HasValue && request.UserPermissions.Contains(f.PermissionRestriction.Value) || f.PermissionRestriction == null)
			.Select(f => f.Id)
			.ToListAsync();
		List<PublicationFlag> existingEditablePublicationFlags = publication.PublicationFlags.Where(pf => editableFlags.Contains(pf.FlagId)).ToList();
		List<int> selectedEditableFlagIds = request.SelectedFlags.Intersect(editableFlags).ToList();

		var flagsToKeep = publication.PublicationFlags.Except(existingEditablePublicationFlags).ToList();
		publication.PublicationFlags.Clear();
		publication.PublicationFlags.AddRange(flagsToKeep);
		publication.PublicationFlags.AddFlags(selectedEditableFlagIds);
		changeMessages.AddRange((await flagService
			.GetDiff(existingEditablePublicationFlags.Select(p => p.FlagId), selectedEditableFlagIds))
			.ToMessages("flags"));

		// Handle tags
		changeMessages.AddRange((await tagService
			.GetDiff(publication.PublicationTags.Select(p => p.TagId), request.SelectedTags))
			.ToMessages("tags"));

		publication.PublicationTags.Clear();
		db.PublicationTags.RemoveRange(
			db.PublicationTags.Where(pt => pt.PublicationId == publication.Id));

		publication.PublicationTags.AddTags(request.SelectedTags);

		await db.SaveChangesAsync();

		// Handle wiki page updates
		var existingWikiPage = await wikiPages.PublicationPage(publicationId);
		IWikiPage? pageToSync = existingWikiPage;

		if (request.WikiMarkup != existingWikiPage!.Markup)
		{
			pageToSync = await wikiPages.Add(new WikiCreateRequest
			{
				PageName = WikiHelper.ToPublicationWikiPageName(publicationId),
				Markup = request.WikiMarkup ?? "",
				MinorEdit = request.MinorEdit,
				RevisionMessage = request.RevisionMessage,
				AuthorId = request.AuthorId
			});
			changeMessages.Add("Description updated");
		}

		// Handle YouTube sync
		foreach (var url in publication.PublicationUrls.ThatAreStreaming())
		{
			if (youtubeSync.IsYoutubeUrl(url.Url))
			{
				await youtubeSync.SyncYouTubeVideo(new YoutubeVideo(
					publicationId,
					publication.CreateTimestamp,
					url.Url!,
					url.DisplayName,
					publication.GenerateTitle(true),
					pageToSync!,
					publication.System!.Code,
					publication.Authors.OrderBy(pa => pa.Ordinal).Select(a => a.Author!.UserName),
					publication.ObsoletedById));
			}
		}

		return new UpdatePublicationResult(true, changeMessages, publication.Title);
	}

	public async Task<List<FileEntry>> GetAvailableMovieFiles(int publicationId)
		=> await db.PublicationFiles
			.ThatAreMovieFiles()
			.ForPublication(publicationId)
			.Select(pf => new FileEntry(pf.Id, pf.Description, pf.Path))
			.ToListAsync();
}

public record UnpublishResult(
	UnpublishResult.UnpublishStatus Status,
	string PublicationTitle,
	string ErrorMessage)
{
	public enum UnpublishStatus { Success, NotFound, NotAllowed }

	internal static UnpublishResult NotFound() => new(UnpublishStatus.NotFound, "", "");

	internal static UnpublishResult HasAwards(string publicationTitle) => new(
		UnpublishStatus.NotAllowed,
		publicationTitle,
		"Cannot unpublish a publication that has awards");

	internal static UnpublishResult Success(string publicationTitle)
		=> new(UnpublishStatus.Success, publicationTitle, "");
}

public record UpdatePublicationRequest(
	int? ObsoletedBy,
	string? EmulatorVersion,
	string? ExternalAuthors,
	List<string> Authors,
	List<int> SelectedFlags,
	List<int> SelectedTags,
	List<PermissionTo> UserPermissions,
	string? WikiMarkup,
	string? RevisionMessage,
	int AuthorId,
	bool MinorEdit);

public record UpdatePublicationResult(bool Success, List<string> ChangeMessages, string? Title = null);

public record FileEntry(int Id, string? Description, string FileName);
