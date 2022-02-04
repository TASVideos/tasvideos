using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.MovieParsers.Result;

// ReSharper disable file CompareOfFloatsByEqualityOperator
namespace TASVideos.Pages.Submissions;

public class SubmissionBasePageModel : BasePageModel
{
	protected readonly ApplicationDbContext Db;

	public SubmissionBasePageModel(ApplicationDbContext db)
	{
		Db = db;
	}

	protected async Task MapParsedResult(IParseResult parseResult, Submission submission)
	{
		if (!parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult);
			return;
		}

		submission.MovieStartType = (int)parseResult.StartType;
		submission.Frames = parseResult.Frames;
		submission.RerecordCount = parseResult.RerecordCount;
		submission.MovieExtension = parseResult.FileExtension;
		submission.System = await Db.GameSystems
			.ForCode(parseResult.SystemCode)
			.SingleOrDefaultAsync();

		if (submission.System == null)
		{
			ModelState.AddModelError("", $"Unknown system type of {parseResult.SystemCode}");
			return;
		}

		if (parseResult.FrameRateOverride.HasValue)
		{
			var frameRate = await Db.GameSystemFrameRates
				.ForSystem(submission.System.Id)
				.FirstOrDefaultAsync(sf => sf.FrameRate == parseResult.FrameRateOverride.Value);

			if (frameRate == null)
			{
				frameRate = new GameSystemFrameRate
				{
					System = submission.System,
					FrameRate = parseResult.FrameRateOverride.Value,
					RegionCode = parseResult.Region.ToString().ToUpper()
				};
				Db.GameSystemFrameRates.Add(frameRate);
				await Db.SaveChangesAsync();
			}

			submission.SystemFrameRate = frameRate;
		}
		else
		{
			// SingleOrDefault should work here because the only time there could be more than one for a system and region are formats that return a framerate override
			// Those systems should never hit this code block.  But just in case.
			submission.SystemFrameRate = await Db.GameSystemFrameRates
				.ForSystem(submission.System.Id)
				.ForRegion(parseResult.Region.ToString())
				.FirstOrDefaultAsync();
		}
	}
}
