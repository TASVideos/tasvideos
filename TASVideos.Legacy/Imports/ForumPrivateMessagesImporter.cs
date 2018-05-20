using System;
using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.ForumEngine;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPrivateMessagesImporter
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			// TODO: attach sig? I'm leaning towards this being pointless
			// TODO: messages without corresponding text
			// TODO: this filters out some messages where the to or from users no longer, should those get imported?
			// TODO: can we filter out message from a non-existent/inactive/banned user to another? neither will ever see it
			var data =
				(from p in legacyForumContext.PrivateMessages
				join pt in legacyForumContext.PrivateMessageText on p.Id equals pt.Id
				join fromUser in legacyForumContext.Users on p.FromUserId equals fromUser.UserId
				where p.ToUserId > 0 && p.FromUserId > 0 // TODO: do we care about these?
				select new
				{
					p.Type,
					p.ToUserId,
					p.FromUserId,
					p.Timestamp,
					p.Subject,
					pt.Text,
					p.EnableBbCode,
					p.EnableHtml,
					p.IpAddress,
					fromUser.UserName
				})
				.ToList();

			var privMessages = data
				.GroupBy(tkey => new
				{
					tkey.ToUserId,
					tkey.FromUserId,
					tkey.Timestamp,
					tkey.Subject,
					tkey.Text,
					tkey.EnableBbCode,
					tkey.EnableHtml,
					tkey.IpAddress,
					tkey.UserName
				})
				.Select(g => new PrivateMessage
				{
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(g.Key.Timestamp),
					CreateUserName = g.Key.UserName,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(g.Key.Timestamp),
					LastUpdateUserName = g.Key.UserName,
					FromUserId = g.Key.FromUserId,
					ToUserId = g.Key.ToUserId,
					IpAddress = g.Key.IpAddress,
					Subject = ImportHelper.FixString(g.Key.Subject),
					Text = ImportHelper.FixString(g.Key.Text),
					EnableHtml = g.Key.EnableHtml && HtmlParser.ContainsHtml(ImportHelper.FixString(g.Key.Text)),
					EnableBbCode = g.Key.EnableBbCode,
					ReadOn = g.All(gg => gg.Type != 1)
						? DateTime.UtcNow  // Legacy system didn't track date so we will simply use the import date
						: (DateTime?)null,
					SavedForFromUser = g.Any(gg => gg.Type == 4),
					SavedForToUser = g.Any(gg => gg.Type == 3)
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

			privMessages.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.PrivateMessages), SqlBulkCopyOptions.Default, 20000, 600);
		}
	}
}
