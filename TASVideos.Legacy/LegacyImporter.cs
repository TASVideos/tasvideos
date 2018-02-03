using System.Linq;
using TASVideos.Data;
using TASVideos.Legacy.Data;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy
{
    public static class LegacyImporter
    {
		public static void RunLegacyImport(ApplicationDbContext context, NesVideosSiteContext legacyContext)
		{
			// For now assume any wiki pages means the importer has run
			if (context.WikiPages.Any())
			{
				return;
			}

			WikiImporter.ImportWikiPages(context, legacyContext);
		}
	}
}
