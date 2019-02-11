using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public class PrivateMessage : BaseEntity
	{
		public int Id { get; set; }

		public int FromUserId { get; set; }
		public virtual User FromUser { get; set; }

		public int ToUserId { get; set; }
		public virtual User ToUser { get; set; }

		[StringLength(50)]
		public string IpAddress { get; set; }

		[StringLength(500)]
		public string Subject { get; set; }

		[Required]
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
		public static IQueryable<PrivateMessage> ToUser(this IQueryable<PrivateMessage> query, int userId)
		{
			return query.Where(m => m.ToUserId == userId);
		}

		public static IQueryable<PrivateMessage> FromUser(this IQueryable<PrivateMessage> query, int userId)
		{
			return query.Where(m => m.FromUserId == userId);
		}

		public static IQueryable<PrivateMessage> ThatAreNotToUserSaved(this IQueryable<PrivateMessage> query)
		{
			return query.Where(m => !m.SavedForToUser);
		}

		public static IQueryable<PrivateMessage> ThatAreNotToUserDeleted(this IQueryable<PrivateMessage> query)
		{
			return query.Where(m => !m.DeletedForToUser);
		}
	}
}
