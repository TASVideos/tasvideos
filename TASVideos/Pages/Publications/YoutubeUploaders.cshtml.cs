using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationMetaData)]
public class YoutubeUploadersModel : BasePageModel
{
	private const string CachePrefix = "YoutubeUploaders-";
	private readonly ApplicationDbContext _db;
	private readonly IYoutubeSync _youtubeSync;
	private readonly ICacheService _cache;

	public YoutubeUploadersModel(ApplicationDbContext db, IYoutubeSync youtubeSync, ICacheService cache)
	{
		_db = db;
		_youtubeSync = youtubeSync;
		_cache = cache;
	}

	public ICollection<YoutubeRecord> Videos { get; set; } = new List<YoutubeRecord>();

	public async Task<IActionResult> OnGet()
	{
		var raw = await _db.PublicationUrls
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
			.Where(r => _youtubeSync.IsYoutubeUrl(r.Url))
			.Select(u => new YoutubeRecord(u.PublicationId, _youtubeSync.VideoId(u.Url!), u.IsObsolete))
			.Distinct()
			.ToList();

		SetChannelTitlesFromCache(Videos);

		var uncachedVideos = Videos
			.Where(v => string.IsNullOrWhiteSpace(v.ChannelTitle))
			.ToList();

		var mapping = (await _youtubeSync
			.GetPublicInfo(uncachedVideos.Select(v => v.VideoId)))
			.ToDictionary(tkey => tkey.Id);

		foreach (var record in uncachedVideos)
		{
			var result = mapping.TryGetValue(record.VideoId, out var val);
			if (result)
			{
				record.ChannelTitle = val!.Snippet.ChannelTitle;
				_cache.Set(CachePrefix + record.VideoId, record.ChannelTitle, Durations.OneDayInSeconds);
			}
		}

		return Page();
	}

	private void SetChannelTitlesFromCache(IEnumerable<YoutubeRecord> records)
	{
		foreach (var record in records)
		{
			var result = _cache.TryGetValue(CachePrefix + record.VideoId, out string val);
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
