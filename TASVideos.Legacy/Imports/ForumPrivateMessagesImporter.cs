using System;
using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPrivateMessagesImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			// TODO: attach sig? I'm leaning towards this being pointless
			// TODO: messages without corresponding text
			// TODO: this filters out some messages where the to or from users no longer, should those get imported?

			var privMessagesTemp =
				(from p in legacyForumContext.PrivateMessages
				 join pt in legacyForumContext.PrivateMessageText on p.Id equals pt.Id
				 join fromUser in legacyForumContext.Users on p.FromUserId equals fromUser.UserId
				 where (p.ToUserId > 0 && p.FromUserId > 0) // TODO: do we care about these?
				 group new { p.Type } by new
				 {
					p.ToUserId,
					p.FromUserId,
					p.Timestamp,
					p.Subject,
					pt.Text,
					p.EnableBbCode,
					p.EnableHtml,
					p.IpAddress,
					fromUser.UserName
				 }
				 into g
				 select new
				 {
					g.Key.ToUserId,
					g.Key.FromUserId,
					CreateUserName = g.Key.UserName,
					g.Key.Timestamp,
					g.Key.Subject,
					g.Key.Text,
					g.Key.EnableBbCode,
					g.Key.EnableHtml,
					g.Key.IpAddress,
					IsRead = g.Any(gg => gg.Type == 0),
					IsNew = g.Any(gg => gg.Type == 1),
					//IsSent = g.Any(gg => gg.Type == 2), // Don't need this one
					IsSavedIn = g.Any(gg => gg.Type == 3),
					IsSavedOut = g.Any(gg => gg.Type == 4),
					IsUnread = g.Any(gg => gg.Type == 5),
				 })
				.ToList();

			var privMessages = privMessagesTemp
				.Select(p => new PrivateMessage
				{
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(p.Timestamp),
					CreateUserName = p.CreateUserName,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(p.Timestamp),
					LastUpdateUserName = p.CreateUserName,
					FromUserId = p.FromUserId,
					ToUserId = p.ToUserId,
					IpAddress = p.IpAddress,
					Subject = ImportHelper.FixString(p.Subject),
					Text = ImportHelper.FixString(p.Text),
					EnableHtml = p.EnableHtml,
					EnableBbCode = p.EnableBbCode,
					ReadOn = !p.IsNew // p.IsUnread // Unread = seen but not read?
						? DateTime.UtcNow  // Legacy system didn't track date so we will simply use the import date
						: (DateTime?)null,
					SavedForFromUser = p.IsSavedOut,
					SavedForToUser = p.IsSavedIn
				})
				.ToList();

			var columns = new[]
			{
				nameof(PrivateMessage.CreateTimeStamp),
				nameof(PrivateMessage.CreateUserName),
				nameof(PrivateMessage.LastUpdateTimeStamp),
				nameof(PrivateMessage.LastUpdateUserName),
				nameof(PrivateMessage.FromUserId),
				nameof(PrivateMessage.ToUserId),
				nameof(PrivateMessage.IpAddress),
				nameof(PrivateMessage.Subject),
				nameof(PrivateMessage.Text),
				nameof(PrivateMessage.EnableHtml),
				nameof(PrivateMessage.EnableBbCode),
				nameof(PrivateMessage.ReadOn),
				nameof(PrivateMessage.SavedForFromUser),
				nameof(PrivateMessage.SavedForToUser),
				nameof(PrivateMessage.DeletedForToUser),
				nameof(PrivateMessage.DeletedForFromUser)
			};

			privMessages.BulkInsert(context, columns, nameof(ApplicationDbContext.PrivateMessages), SqlBulkCopyOptions.Default, 20000);
		}
	}
}
