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
			// TODO: attach sig
			// TODO: types, I think this corresponds to things like "sent, read, etc"
			// TODO: messages without corresponding text
			// TODO: this filters out some messages where the to or from users no longer, should those get imported?
			var privMessages =
				(from p in legacyForumContext.PrivateMessages
				 join pt in legacyForumContext.PrivateMessageText on p.Id equals pt.Id
				 join fromUser in legacyForumContext.Users on p.FromUserId equals fromUser.UserId
				 select new
				 {
					 p.Id,
					 p.ToUserId,
					 p.FromUserId,
					 CreateUserName = fromUser.UserName,
					 p.Timestamp,
					 p.Subject,
					 pt.Text,
					 p.EnableBbCode,
					 p.EnableHtml,
					 p.Type,
					 p.IpAddress
				 })
				.ToList()
				.Select(p => new ForumPrivateMessage
				{
					Id = p.Id,
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
					EnableBbCode = p.EnableBbCode
				})
				.ToList();

			var columns = new[]
			{
				nameof(ForumPrivateMessage.Id),
				nameof(ForumPrivateMessage.CreateTimeStamp),
				nameof(ForumPrivateMessage.CreateUserName),
				nameof(ForumPrivateMessage.LastUpdateTimeStamp),
				nameof(ForumPrivateMessage.LastUpdateUserName),
				nameof(ForumPrivateMessage.FromUserId),
				nameof(ForumPrivateMessage.ToUserId),
				nameof(ForumPrivateMessage.IpAddress),
				nameof(ForumPrivateMessage.Subject),
				nameof(ForumPrivateMessage.Text),
				nameof(ForumPrivateMessage.EnableHtml),
				nameof(ForumPrivateMessage.EnableBbCode)
			};

			privMessages.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumPrivateMessages), SqlBulkCopyOptions.KeepIdentity, 20000);
		}
	}
}
