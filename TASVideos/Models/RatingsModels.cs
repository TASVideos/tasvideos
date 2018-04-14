using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Models
{
	public class PublicationRatingsViewModel
	{
		public int PublicationId { get; set; }
		public string PublicationTitle { get; set; }

		public IEnumerable<RatingEntry> Ratings { get; set; } = new List<RatingEntry>();

		public double AverageEntertainmentRating =>
			Math.Round(Ratings
				.Where(r => r.Entertainment.HasValue)
				.Select(r => r.Entertainment.Value).Average(), 2);

		public double AverageTechRating =>
			Math.Round(Ratings
				.Where(r => r.TechQuality.HasValue)
				.Select(r => r.Entertainment.Value).Average(), 2);

		// Entertainmnet counts 2:1 over Tech
		public double OverallRating => Math.Round(Ratings
				.Where(r => r.Entertainment.HasValue)
				.Select(r => r.Entertainment.Value)
				.Concat(Ratings
					.Where(r => r.Entertainment.HasValue)
					.Select(r => r.Entertainment.Value))
				.Concat(Ratings
					.Where(r => r.TechQuality.HasValue)
					.Select(r => r.TechQuality.Value))
				.Average(), 2);

		public class RatingEntry
		{
			[Display(Name = "UserName")]
			public string UserName { get; set; }
			
			[Display(Name = "Entertainment")]
			public double? Entertainment { get; set; }

			[Display(Name = "Tech Quality")]
			public double? TechQuality { get; set; }

			public bool IsPublic { get; set; }
		}
	}
}
