using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Legacy.Imports
{
	public static class SubmissionFrameRateImporter
	{
		private const decimal RoundingOffset = 0.01M;

		public static void Import(ApplicationDbContext context)
		{ 
			var systemFrameRates = context.GameSystemFrameRates
				.Include(sf => sf.System)
				.ToList();

			var submissions = context.Submissions
				.Include(s => s.System)
				.ToList();

			var groups = from s in submissions
					join sf in systemFrameRates on s.SystemId equals sf.GameSystemId
					group sf by s into g
					select g;

			foreach (var g in groups)
			{
				var legacyFrameRate = g.Key.LegacyTime > 0 // Art Alive
					? g.Key.Frames / g.Key.LegacyTime
					: 60;

				var systemFrameRate = g
					.FirstOrDefault(sf => Math.Abs(((decimal)sf.FrameRate) - legacyFrameRate) < RoundingOffset);

				if (systemFrameRate == null)
				{
					systemFrameRate = new GameSystemFrameRate
					{
						System = g.Key.System,
						FrameRate = (double)legacyFrameRate,
						RegionCode = "NTSC",
						Obsolete = g.Key.SystemId != 38 // We want Linux framerates by design, so they should not be flagged
					};
					g.Key.SystemFrameRate = systemFrameRate;
					context.GameSystemFrameRates.Add(systemFrameRate);
				}
				else
				{
					g.Key.SystemFrameRateId = g.First().Id;
					g.Key.SystemFrameRate = g.First();
				}
				
				g.Key.ImportedTime = decimal.Round((decimal)(g.Key.Frames / systemFrameRate.FrameRate), 3);
				g.Key.GenerateTitle();
			}
			
			context.SaveChanges();
		}
	}
}
