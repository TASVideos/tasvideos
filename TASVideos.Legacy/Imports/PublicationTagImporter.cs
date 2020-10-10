using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class PublicationTagImporter
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var movieTags = legacySiteContext.MovieClass.ToList();
			var legacyClassTypes = legacySiteContext.ClassTypes.ToList();
			var tags = context.Tags.Select(t => new { t.Id, t.DisplayName }).ToList();

			var publicationTags = new List<PublicationTag>();

			foreach (var mc in movieTags)
			{
				var classType = mc.ClassId >= 1000
						? legacyClassTypes.Single(c => c.Id == mc.ClassId)
						: legacyClassTypes.Single(c => c.OldId == mc.ClassId);

					if (classType.PositiveText.Contains("Genre"))
					{
						continue;
					}

					var tag = mc.Value == 1
						? tags.Single(t => t.DisplayName == classType.PositiveText)
						: tags.Single(t => t.DisplayName == classType.NegativeText);

					publicationTags.Add(new PublicationTag
					{
						PublicationId = mc.MovieId,
						TagId = tag.Id
					});
			}

			var pubTagColumns = new[]
			{
				nameof(PublicationTag.PublicationId),
				nameof(PublicationTag.TagId)
			};

			publicationTags.BulkInsert(connectionStr, pubTagColumns, nameof(ApplicationDbContext.PublicationTags), SqlBulkCopyOptions.Default);
		}
	}
}
