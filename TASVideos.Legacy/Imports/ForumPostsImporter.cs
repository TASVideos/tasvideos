using System.Data.SqlClient;
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
			var posts = legacyForumContext.Posts
				.Select(p => new
				{
					p.Id,
						p.TopicId,
						p.IpAddress,
						p.Timestamp,
						p.PostText.Subject,
						p.PostText.Text,
						p.EnableBbCode,
						p.EnableHtml,
						p.LastUpdateTimestamp,
						LastUpdateUserName = p.LastUpdateUser.UserName,
						p.PostText.BbCodeUid,
						p.PosterId,
						PosterName = p.Poster.UserName
				})
				.ToList()
				.Select(p =>
				{
					var fixedText = System.Web.HttpUtility.HtmlDecode(ImportHelper.ConvertLatin1String(p.Text.Replace(":" + p.BbCodeUid, "")));

					return new ForumPost
					{
						Id = p.Id,
						TopicId = p.TopicId,
						PosterId = p.PosterId,
						IpAddress = p.IpAddress,
						Subject = WebUtility.HtmlDecode(ImportHelper.ConvertLatin1String(p.Subject)),
						Text = fixedText,
						EnableBbCode = p.EnableBbCode,
						EnableHtml = p.EnableHtml && BbParser.ContainsHtml(fixedText, p.EnableBbCode),
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
