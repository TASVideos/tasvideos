using System.Linq;

using TASVideos.Data;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class SubmissionImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var legacySubmissions = legacySiteContext.Submissions
				.Where(s => s.Id > 0)
				.ToList();

			int zzz = 0;
		}
	}
}
