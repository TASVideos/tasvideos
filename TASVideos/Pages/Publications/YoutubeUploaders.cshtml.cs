using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationMetaData)]
public class YoutubeUploadersModel(ApplicationDbContext db, IYoutubeSync youtubeSync, ICacheService cache) : BasePageModel
{
	private const string CachePrefix = "YoutubeUploaders-";

	public List<YoutubeEntry> Videos { get; set; } = [];

	public async Task OnGet()
	{
		var raw = await db.PublicationUrls
			.ThatAreStreaming()
			.Where(u => u.PublicationId > 0)
			.Select(u => new
			{
				u.Url,
				u.PublicationId,
				u.Publication!.Title,
				IsObsolete = u.Publication!.ObsoletedById.HasValue
			})
			.ToListAsync();

		Videos = raw
			.Where(r => youtubeSync.IsYoutubeUrl(r.Url))
			.Select(u => new YoutubeEntry(u.PublicationId, u.Title, youtubeSync.VideoId(u.Url!), u.IsObsolete))
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
				cache.Set(CachePrefix + record.VideoId, record.ChannelTitle, Durations.OneDay);
			}
		}
	}

	private void SetChannelTitlesFromCache(IEnumerable<YoutubeEntry> records)
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

	public record YoutubeEntry(int PublicationId, string PublicationTitle, string VideoId, bool IsObsolete)
	{
		public string ChannelTitle { get; set; } = "";
	}
}
