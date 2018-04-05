using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

using TASVideos.Data.Entity.Forum;

namespace TASVideos.Data.Entity
{
	[Table(nameof(User))]
	public class User : IdentityUser<int>, ITrackable
	{
		public DateTime? LastLoggedInTimeStamp { get; set; }
		public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id;

		public DateTime CreateTimeStamp { get; set; } = DateTime.UtcNow;
		public string CreateUserName { get; set; }
		public DateTime LastUpdateTimeStamp { get; set; } = DateTime.UtcNow;
		public string LastUpdateUserName { get; set; }

		public string Avatar { get; set; }
		public string From { get; set; }

		public string LegacyPassword { get; set; }

		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
		public virtual ICollection<SubmissionAuthor> Submissions { get; set; } = new HashSet<SubmissionAuthor>();

		public virtual ICollection<ForumTopic> Topics { get; set; } = new HashSet<ForumTopic>();
		public virtual ICollection<ForumPost> Posts { get; set; } = new HashSet<ForumPost>();
	}
}
