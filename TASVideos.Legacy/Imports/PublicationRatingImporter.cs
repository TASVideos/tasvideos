﻿using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal static class PublicationRatingImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var ratingsDto = legacySiteContext.MovieRating
				.Select(mr => new
				{
					UserName = mr.User!.Name,
					PublicationId = mr.MovieId,
					Type = mr.RatingName == "Entertainment"
						? PublicationRatingType.Entertainment
						: PublicationRatingType.TechQuality,
					mr.Value
				})
				.ToList();

			var usersWithRatings = ratingsDto
				.Select(u => u.UserName)
				.Distinct()
				.ToList();

			var users = context.Users
				.Where(u => usersWithRatings.Contains(u.UserName))
				.Select(u => new { u.Id, u.UserName })
				.ToList();

			var ratings = (from r in ratingsDto
				join u in users on r.UserName.ToLower() equals u.UserName.ToLower()
				select new PublicationRating
				{
					UserId = u.Id,
					PublicationId = r.PublicationId,
					Type = r.Type,
					Value = (double)r.Value
				})
				.ToList();

			var columns = new[]
			{
				nameof(PublicationRating.UserId),
				nameof(PublicationRating.PublicationId),
				nameof(PublicationRating.Type),
				nameof(PublicationRating.Value)
			};

			ratings.BulkInsert(columns, nameof(ApplicationDbContext.PublicationRatings));
		}
	}
}
