using TASVideos.Common;
using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Frames)]
[TextModule]
public class Frames(ApplicationDbContext db, ICacheService cache) : WikiViewComponent
{
	private const string CacheKey = "FramesModule";
	public Timeable Time { get; set; } = new();

	public async Task<string> RenderTextAsync(IWikiPage? pageData, double? fps, int amount)
	{
		var model = new Timeable
		{
			Frames = amount,
			FrameRate = fps ?? await GuessFps(pageData?.PageName)
		};

		return model.Time().ToStringWithOptionalDaysAndHours();
	}

	public async Task<IViewComponentResult> InvokeAsync(IWikiPage? pageData, double? fps, int amount)
	{
		Time = new Timeable
		{
			Frames = amount,
			FrameRate = fps ?? await GuessFps(pageData?.PageName)
		};

		return View();
	}

	private async ValueTask<double> GuessFps(string? pageName)
	{
		if (WikiHelper.IsSubmissionPage(pageName, out var submissionId))
		{
			var sub = await db.Submissions
				.Where(s => s.Id == submissionId)
				.Select(s => new { s.Id, s.SystemFrameRate!.FrameRate })
				.SingleOrDefaultAsync(s => s.Id == submissionId);

			if (sub?.FrameRate is not null)
			{
				return sub.FrameRate;
			}

			return 60;
		}

		if (WikiHelper.IsPublicationPage(pageName, out var publicationId))
		{
			var key = $"{CacheKey}{publicationId}";
			if (cache.TryGetValue(key, out double frameRate))
			{
				return frameRate;
			}

			var pub = await db.Publications
				.Where(p => p.Id == publicationId)
				.Select(p => new { p.Id, p.SystemFrameRate!.FrameRate })
				.SingleOrDefaultAsync();

			if (pub?.FrameRate is not null)
			{
				cache.Set(key, pub.FrameRate);
				return pub.FrameRate;
			}
		}

		return 60;
	}
}
