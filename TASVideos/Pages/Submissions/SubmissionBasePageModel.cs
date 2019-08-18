using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions
{
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

			submission.SystemFrameRate = await Db.GameSystemFrameRates
				.ForSystem(submission.System.Id)
				.ForRegion(parseResult.Region.ToString())
				.SingleOrDefaultAsync();
		}
	}
}
