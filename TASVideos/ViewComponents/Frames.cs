using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Common;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Frames)]
[TextModule]
public class Frames : ViewComponent
{
	private const string CacheKey = "FramesModule";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;

	public Frames(ApplicationDbContext db, ICacheService cache)
	{
		_db = db;
		_cache = cache;
	}

	public async Task<string> RenderTextAsync(WikiPage? pageData, double? fps, int amount)
	{
		var model = new Timeable
		{
			Frames = amount,
			FrameRate = fps ?? await GuessFps(pageData?.PageName)
		};

		return model.Time().ToStringWithOptionalDaysAndHours();
	}

	public async Task<IViewComponentResult> InvokeAsync(WikiPage? pageData, double? fps, int amount)
	{
		var model = new Timeable
		{
			Frames = amount,
			FrameRate = fps ?? await GuessFps(pageData?.PageName)
		};

		return View(model);
	}

	private async ValueTask<double> GuessFps(string? pageName)
	{
		var submissionId = WikiHelper.IsSubmissionPage(pageName);
		if (submissionId.HasValue)
		{
			var sub = await _db.Submissions
				.Where(s => s.Id == submissionId.Value)
				.Select(s => new { s.Id, s.SystemFrameRate!.FrameRate })
				.SingleOrDefaultAsync(s => s.Id == submissionId.Value);

			if (sub?.FrameRate is not null)
			{
				return sub.FrameRate;
			}

			return 60;
		}

		var publicationId = WikiHelper.IsPublicationPage(pageName);
		if (publicationId.HasValue)
		{
			var key = CacheKey + publicationId.Value;
			if (_cache.TryGetValue(key, out double frameRate))
			{
				return frameRate;
			}

			var pub = await _db.Publications
				.Where(p => p.Id == publicationId.Value)
				.Select(p => new { p.Id, p.SystemFrameRate!.FrameRate })
				.SingleOrDefaultAsync();

			if (pub?.FrameRate is not null)
			{
				_cache.Set(key, pub.FrameRate);
				return pub.FrameRate;
			}
		}

		return 60;
	}
}
