using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

using TASVideos.Data.Entity.Awards;
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
		public string Signature { get; set; }

		public bool PublicRatings { get; set; } = true;

		public string LegacyPassword { get; set; }

		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
		public virtual ICollection<SubmissionAuthor> Submissions { get; set; } = new HashSet<SubmissionAuthor>();
		public virtual ICollection<PublicationAuthor> Publications { get; set; } = new HashSet<PublicationAuthor>();

		public virtual ICollection<ForumTopic> Topics { get; set; } = new HashSet<ForumTopic>();
		public virtual ICollection<ForumPost> Posts { get; set; } = new HashSet<ForumPost>();

		public virtual ICollection<UserAward> UserAwards { get; set; } = new HashSet<UserAward>();

		[ForeignKey(nameof(PrivateMessage.FromUserId))]
		public virtual ICollection<PrivateMessage> SentPrivateMessages { get; set; } = new HashSet<PrivateMessage>();

		[ForeignKey(nameof(PrivateMessage.ToUserId))]
		public virtual ICollection<PrivateMessage> ReceivedPrivateMessages { get; set; } = new HashSet<PrivateMessage>();

		public virtual ICollection<PublicationRating> PublicationRatings { get; set; } = new HashSet<PublicationRating>();

		public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();
		public virtual ICollection<UserFileComment> UserFileComments { get; set; } = new HashSet<UserFileComment>();
	}
}
