using System;
using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumPrivateMessage : BaseEntity
	{
		public int Id { get; set; }

		public int FromUserId { get; set; }
		public virtual User FromUser { get; set; }

		public int ToUserId { get; set; }
		public virtual User ToUser { get; set; }

		public string IpAddress { get; set; }

		public string Subject { get; set; }
		public string Text { get; set; }

		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }

		public DateTime? ReadOn { get; set; } // Only a flag in the legacy system, so the date is the import date for legacy messages
		public bool FromUserSaved { get; set; }
		public bool ToUserSaved { get; set; }
	}

	public static class MessageExtensions
	{
		public static IQueryable<ForumPrivateMessage> ToUser(this IQueryable<ForumPrivateMessage> query, User user)
		{
			return query.Where(m => m.ToUserId == user.Id);
		}

		public static IQueryable<ForumPrivateMessage> FromUser(this IQueryable<ForumPrivateMessage> query, User user)
		{
			return query.Where(m => m.FromUserId == user.Id);
		}

		public static IQueryable<ForumPrivateMessage> ThatAreNotToUserSaved(this IQueryable<ForumPrivateMessage> query)
		{
			return query.Where(m => !m.ToUserSaved);
		}
	}
}
