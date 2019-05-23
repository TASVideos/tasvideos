using System;
using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity;
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
			var data = legacyForumContext.PrivateMessages
				.Where(p => p.ToUserId > 0 && p.FromUserId > 0) // These include delete users, and delete messages, the legacy system puts a negative on user id to soft delete
				.Select(p => new
				{
					p.Type,
					p.ToUserId,
					p.FromUserId,
					p.Timestamp,
					p.Subject,
					p.PrivateMessageText.Text,
					p.EnableBbCode,
					p.EnableHtml,
					p.IpAddress,
					p.PrivateMessageText.BbCodeUid,
					p.FromUser.UserName
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
					tkey.BbCodeUid,
					tkey.EnableBbCode,
					tkey.EnableHtml,
					tkey.IpAddress,
					tkey.UserName
				})
				.Select(g =>
				{
					var fixedText = System.Web.HttpUtility.HtmlDecode(ImportHelper.ConvertLatin1String(g.Key.Text.Replace(":" + g.Key.BbCodeUid, "")));
					return new PrivateMessage
					{
						CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(g.Key.Timestamp),
						CreateUserName = g.Key.UserName,
						LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(g.Key.Timestamp),
						LastUpdateUserName = g.Key.UserName,
						FromUserId = g.Key.FromUserId,
						ToUserId = g.Key.ToUserId,
						IpAddress = g.Key.IpAddress,
						Subject = ImportHelper.ConvertLatin1String(g.Key.Subject),
						Text = fixedText,
						EnableHtml = g.Key.EnableHtml && BbParser.ContainsHtml(fixedText, g.Key.EnableBbCode),
						EnableBbCode = g.Key.EnableBbCode,
						ReadOn = g.All(gg => gg.Type != 1)
							? DateTime.UtcNow // Legacy system didn't track date so we will simply use the import date
							: (DateTime?)null,
						SavedForFromUser = g.Any(gg => gg.Type == 4),
						SavedForToUser = g.Any(gg => gg.Type == 3)
					};
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
