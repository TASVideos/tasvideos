using System;
using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public class PrivateMessage : BaseEntity
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
		public bool SavedForFromUser { get; set; }
		public bool SavedForToUser { get; set; }

		public bool DeletedForFromUser { get; set; }
		public bool DeletedForToUser { get; set; }
	}

	public static class MessageExtensions
	{
		public static IQueryable<PrivateMessage> ToUser(this IQueryable<PrivateMessage> query, User user)
		{
			return query.Where(m => m.ToUserId == user.Id);
		}

		public static IQueryable<PrivateMessage> FromUser(this IQueryable<PrivateMessage> query, User user)
		{
			return query.Where(m => m.FromUserId == user.Id);
		}

		public static IQueryable<PrivateMessage> ThatAreNotToUserSaved(this IQueryable<PrivateMessage> query)
		{
			return query.Where(m => !m.SavedForToUser);
		}

		public static IQueryable<PrivateMessage> ThatAreToNotUserDeleted(this IQueryable<PrivateMessage> query)
		{
			return query.Where(m => !m.DeletedForToUser);
		}
	}
}
