using TASVideos.Common;
using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Frames)]
[TextModule]
public class Frames(ApplicationDbContext db, ICacheService cache) : ViewComponent
{
	private const string CacheKey = "FramesModule";

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
			var sub = await db.Submissions
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
			if (cache.TryGetValue(key, out double frameRate))
			{
				return frameRate;
			}

			var pub = await db.Publications
				.Where(p => p.Id == publicationId.Value)
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
