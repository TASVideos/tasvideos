using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
    public static class TagImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var legacyClassTypes = legacySiteContext.ClassTypes.ToList();
			
			List<Tag> tags = new List<Tag>();
			foreach (var classType in legacyClassTypes.Where(t => !t.PositiveText.Contains("Genre")))
			{
				tags.Add(new Tag
				{
					Code = classType.Abbreviation,
					DisplayName = classType.PositiveText
				});

				if (classType.NegativeText != "N/A")
				{
					tags.Add(new Tag
					{
						Code = $"no{classType.Abbreviation}",
						DisplayName = classType.NegativeText
					});
				}
			}

			var columns = new[]
			{
				nameof(Tag.Code),
				nameof(Tag.DisplayName)
			};

			tags.BulkInsert(context, columns, nameof(ApplicationDbContext.Tags), SqlBulkCopyOptions.Default);
		}
	}
}
