using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TASVideos.Data;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
    public static class PublicationFlagImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var movieFlags = legacySiteContext.MovieFlags
				.Where(mf => mf.FlagId != 3) // AVGN
				.ToList();
		}
	}
}
