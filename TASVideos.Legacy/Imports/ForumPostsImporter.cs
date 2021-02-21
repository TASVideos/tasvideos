using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.ForumEngine;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPostsImporter
	{
		public static void Import(string connectionStr, NesVideosForumContext legacyForumContext)
		{
			// TODO: posts without a corresponding post text
			const int tvaId = 505;
			var posts = legacyForumContext.Posts
				.Where(p => p.PosterId != tvaId || !p.PostText!.Text!.StartsWith("This is an automatically posted message for discussing submission:"))
				.Select(p => new
				{
					p.Id,
					p.TopicId,
					p.IpAddress,
					p.Timestamp,
					p.PostText!.Subject,
					p.PostText.Text,
					p.EnableBbCode,
					p.EnableHtml,
					p.LastUpdateTimestamp,
					LastUpdateUserName = p.LastUpdateUser!.UserName,
					p.PostText.BbCodeUid,
					p.PosterId,
					PosterName = p.Poster!.UserName,
					p.MoodAvatar
				})
				.ToList()
				.Select(p =>
				{
					bool enableBbCode = p.EnableBbCode;
					bool enableHtml;

					string fixedText;
					if (p.PosterId == SiteGlobalConstants.TASVideoAgentId && p.Subject == SiteGlobalConstants.NewPublicationPostSubject)
					{
						enableBbCode = true;
						enableHtml = false;
						var pText = p.Text ?? "";

						// Have to handle old and new style posts
						var temp = pText.Contains("movies.cgi")
							? pText.Split(".cgi")[1]
							: pText.Split(".html")[0];

						var digitsOnly = new Regex(@"[^\d]");
						var publicationId = int.Parse(digitsOnly.Replace(temp, ""));

						fixedText = SiteGlobalConstants.NewPublicationPost.Replace("{PublicationId}", publicationId.ToString());
					}
					else
					{
						fixedText = HttpUtility.HtmlDecode(
							ImportHelper.ConvertLatin1String(p.Text!
								.Replace(":1:" + p.BbCodeUid, "")
								.Replace(":" + p.BbCodeUid, "")) ?? ""
								.Replace("[/list:u]", "[/list]")
							);

						enableHtml = p.EnableHtml && BbParser.ContainsHtml(fixedText, p.EnableBbCode);
					}

					int moodAvatar = p.MoodAvatar == 255 // This seems to just mean normal
						? 1
						: p.MoodAvatar;

					return new ForumPost
					{
						Id = p.Id,
						TopicId = p.TopicId,
						PosterId = p.PosterId,
						IpAddress = p.IpAddress.IpFromHex(),
						Subject = WebUtility.HtmlDecode(ImportHelper.ConvertLatin1String(p.Subject)),
						Text = fixedText,
						EnableBbCode = enableBbCode,
						EnableHtml = enableHtml,
						CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(p.Timestamp),
						LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(p.LastUpdateTimestamp
							?? p.Timestamp),
						CreateUserName = !string.IsNullOrWhiteSpace(p.PosterName) ? p.PosterName : "Unknown",
						LastUpdateUserName = !string.IsNullOrWhiteSpace(p.LastUpdateUserName)
							? p.LastUpdateUserName
							: !string.IsNullOrWhiteSpace(p.PosterName)
								? p.PosterName
								: "Unknown",
						PosterMood = (ForumPostMood)moodAvatar
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
				nameof(ForumPost.EnableBbCode),
				nameof(ForumPost.PosterMood)
			};

			posts.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.ForumPosts), 20000, 600);
		}
	}
}
