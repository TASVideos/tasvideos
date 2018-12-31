using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class ProfileIndexModel
	{
		public string Username { get; set; }

		public bool IsEmailConfirmed { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Display(Name = "Time Zone")]
		public string TimeZoneId { get; set; }

		public string StatusMessage { get; set; }

		[Display(Name = "Allow Movie Ratings to be public?")]
		public bool PublicRatings { get; set; }

		[Display(Name = "Location")]
		public string From { get; set; }

		public IEnumerable<RoleBasicDisplay> Roles { get; set; } = new List<RoleBasicDisplay>();
	}

	public class WatchedTopicsModel
	{
		public DateTime TopicCreateTimeStamp { get; set; }
		public bool IsNotified { get; set; }
		public int ForumId { get; set; }
		public string ForumTitle { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; }
	}
}
