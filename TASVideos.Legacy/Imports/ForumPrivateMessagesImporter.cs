using System;
using System.Linq;
using System.Web;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.ForumEngine;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	internal static class ForumPrivateMessagesImporter
	{
		public static void Import(NesVideosForumContext legacyForumContext)
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
					p.PrivateMessageText!.Text,
					p.EnableBbCode,
					p.EnableHtml,
					p.IpAddress,
					p.PrivateMessageText.BbCodeUid,
					p.FromUser!.UserName
				})
				.ToList();

			var privMessages = data
				.GroupBy(tkey => new
				{
					tkey.ToUserId,
					tkey.FromUserId,
					tkey.Timestamp,
					tkey.Subject,
					Text = tkey.Text ?? "",
					tkey.BbCodeUid,
					tkey.EnableBbCode,
					tkey.EnableHtml,
					tkey.IpAddress,
					tkey.UserName
				})
				.Select(g =>
				{
					var fixedText = HttpUtility.HtmlDecode(ImportHelper.ConvertLatin1String(g.Key.Text.Replace(":" + g.Key.BbCodeUid, ""))) ?? "";
					return new PrivateMessage
					{
						CreateTimestamp = ImportHelper.UnixTimeStampToDateTime(g.Key.Timestamp),
						CreateUserName = g.Key.UserName,
						LastUpdateTimestamp = ImportHelper.UnixTimeStampToDateTime(g.Key.Timestamp),
						LastUpdateUserName = g.Key.UserName,
						FromUserId = g.Key.FromUserId,
						ToUserId = g.Key.ToUserId,
						IpAddress = g.Key.IpAddress.IpFromHex(),
						Subject = ImportHelper.ConvertLatin1String(g.Key.Subject),
						Text = fixedText,
						EnableHtml = g.Key.EnableHtml && BbParser.ContainsHtml(fixedText, g.Key.EnableBbCode),
						EnableBbCode = g.Key.EnableBbCode,
						ReadOn = g.All(gg => gg.Type != 1)
							? DateTime.UtcNow // Legacy system didn't track date so we will simply use the import date
							: null,
						SavedForFromUser = g.Any(gg => gg.Type == 4),
						SavedForToUser = g.Any(gg => gg.Type == 3)
					};
				})
				.ToList();

			var columns = new[]
			{
				nameof(PrivateMessage.CreateTimestamp),
				nameof(PrivateMessage.CreateUserName),
				nameof(PrivateMessage.LastUpdateTimestamp),
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

			privMessages.BulkInsert(columns, nameof(ApplicationDbContext.PrivateMessages));
		}
	}
}
