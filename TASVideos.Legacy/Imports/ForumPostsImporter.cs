﻿using System.Data.SqlClient;
using System.Linq;
using System.Net;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.ForumEngine;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPostsImporter
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			// TODO: posts without a corresponding post text
			var posts = (from p in legacyForumContext.Posts
					join pt in legacyForumContext.PostsText on p.Id equals pt.Id
					join pu in legacyForumContext.Users on p.PosterId equals pu.UserId into ppu
					from pu in ppu.DefaultIfEmpty()
					join lu in legacyForumContext.Users on p.LastUpdateUserId equals lu.UserId into plu
					from lu in plu.DefaultIfEmpty()
					select new
					{
						p.Id,
						p.TopicId,
						p.IpAddress,
						p.Timestamp,
						pt.Subject,
						pt.Text,
						p.EnableBbCode,
						p.EnableHtml,
						p.LastUpdateTimestamp,
						LastUpdateUserName = lu.UserName,
						pt.BbCodeUid,
						p.PosterId,
						PosterName = pu.UserName
					})
				.ToList()
				.Select(p =>
				{
					var fixedText = ImportHelper.ConvertLatin1String(p.Text.Replace(":" + p.BbCodeUid, ""));
					return new ForumPost
					{
						Id = p.Id,
						TopicId = p.TopicId,
						PosterId = p.PosterId,
						IpAddress = p.IpAddress,
						Subject = WebUtility.HtmlDecode(ImportHelper.ConvertLatin1String(p.Subject)),
						Text = fixedText,
						EnableBbCode = p.EnableBbCode,
						EnableHtml = p.EnableHtml && HtmlParser.ContainsHtml(fixedText),
						CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(p.Timestamp),
						LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(p.LastUpdateTimestamp
								?? p.Timestamp),
						CreateUserName = !string.IsNullOrWhiteSpace(p.PosterName) ? p.PosterName : "Unknown",
						LastUpdateUserName = !string.IsNullOrWhiteSpace(p.LastUpdateUserName)
							? p.LastUpdateUserName
							: !string.IsNullOrWhiteSpace(p.PosterName)
								? p.PosterName
								: "Unknown"
					};
				});

			var columns = new[]
			{
				nameof(ForumPost.Id),
				nameof(ForumPost.TopicId),
				nameof(ForumPost.PosterId),
				nameof(ForumPost.IpAddress),
				nameof(ForumPost.Subject),
				nameof(ForumPost.Text),
				nameof(ForumPost.CreateTimeStamp),
				nameof(ForumPost.CreateUserName),
				nameof(ForumPost.LastUpdateTimeStamp),
				nameof(ForumPost.LastUpdateUserName),
				nameof(ForumPost.EnableHtml),
				nameof(ForumPost.EnableBbCode)
			};

			posts.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.ForumPosts), SqlBulkCopyOptions.KeepIdentity, 20000, 600);
		}
	}
}
