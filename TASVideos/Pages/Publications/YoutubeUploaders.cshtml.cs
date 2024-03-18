using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationMetaData)]
public class YoutubeUploadersModel(ApplicationDbContext db, IYoutubeSync youtubeSync, ICacheService cache)
	: BasePageModel
{
	private const string CachePrefix = "YoutubeUploaders-";

	public IReadOnlyCollection<YoutubeRecord> Videos { get; set; } = new List<YoutubeRecord>();

	public async Task<IActionResult> OnGet()
	{
		var raw = await db.PublicationUrls
			.ThatAreStreaming()
			.Where(u => u.PublicationId > 0)
			.Select(u => new
			{
				u.Url,
				u.PublicationId,
				IsObsolete = u.Publication!.ObsoletedById.HasValue
			})
			.ToListAsync();

		Videos = raw
			.Where(r => youtubeSync.IsYoutubeUrl(r.Url))
			.Select(u => new YoutubeRecord(u.PublicationId, youtubeSync.VideoId(u.Url!), u.IsObsolete))
			.Distinct()
			.ToList();

		SetChannelTitlesFromCache(Videos);

		var uncachedVideos = Videos
			.Where(v => string.IsNullOrWhiteSpace(v.ChannelTitle))
			.ToList();

		var mapping = (await youtubeSync
			.GetPublicInfo(uncachedVideos.Select(v => v.VideoId)))
			.ToDictionary(tkey => tkey.Id);

		foreach (var record in uncachedVideos)
		{
			var result = mapping.TryGetValue(record.VideoId, out var val);
			if (result)
			{
				record.ChannelTitle = val!.Snippet.ChannelTitle;
				cache.Set(CachePrefix + record.VideoId, record.ChannelTitle, Durations.OneDayInSeconds);
			}
		}

		return Page();
	}

	private void SetChannelTitlesFromCache(IEnumerable<YoutubeRecord> records)
	{
		foreach (var record in records)
		{
			var result = cache.TryGetValue(CachePrefix + record.VideoId, out string val);
			if (result)
			{
				record.ChannelTitle = val;
			}
		}
	}

	public record YoutubeRecord(int PublicationId, string VideoId, bool IsObsolete)
	{
		public string ChannelTitle { get; set; } = "";
	}
}
