using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class PublicationRatingsViewModel
	{
		public int PublicationId { get; set; }
		public string PublicationTitle { get; set; }

		public IEnumerable<RatingEntry> Ratings { get; set; } = new List<RatingEntry>();

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
