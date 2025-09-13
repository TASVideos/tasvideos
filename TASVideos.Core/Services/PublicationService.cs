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
	/// Returns whether a publication can be unpublished, does not affect the publication
	/// </summary>
	Task<UnpublishResult> CanUnpublish(int publicationId);

	/// <summary>
	/// Deletes a publication and returns the corresponding submission back to the submission queue
	/// </summary>
	Task<UnpublishResult> Unpublish(int publicationId);
}

internal class Publications(
	ApplicationDbContext db,
	IYoutubeSync youtubeSync,
	ITASVideoAgent tva,
	IWikiPages wikiPages)
	: IPublications
{
	public Task<string?> GetTitle(int publicationId)
		=> db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

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
