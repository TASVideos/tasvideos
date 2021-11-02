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
	internal static class ForumPostsImporter
	{
		public static void Import(NesVideosForumContext legacyForumContext)
		{
			// TODO: posts without a corresponding post text
			var posts = legacyForumContext.Posts
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
					p.MoodAvatar,
					p.Topic!.ForumId
				})
				.ToList()
				.Select(p =>
				{
					bool enableBbCode = p.EnableBbCode;
					bool enableHtml;
					string fixedText;
					string? decodedSubject = WebUtility.HtmlDecode(ImportHelper.ConvertLatin1String(p.Subject));

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
					else if (p.PosterId == SiteGlobalConstants.TASVideoAgentId && p.Text is not null && p.Text.StartsWith("This is an automatically posted message for discussing submission:"))
					{
						enableBbCode = true;
						enableHtml = false;

						string submissionId = "";

						// Two posts (Ids: 222420, 295274) with their topics deleted have an empty subject line and link to no submission
						if (!string.IsNullOrEmpty(decodedSubject))
						{
							// Submission subjects always start like #1234:
							submissionId = decodedSubject[1..decodedSubject.IndexOf(":")];
						}

						fixedText = $"{SiteGlobalConstants.NewSubmissionPost}[submission]{submissionId}[/submission]";
						decodedSubject = null;
					}
					else
					{
						fixedText = HttpUtility.HtmlDecode(
							ImportHelper.ConvertLatin1String(p.Text!
								.Replace(":1:" + p.BbCodeUid, "")
								.Replace(":" + p.BbCodeUid, "")
								.Replace("[/list:u]", "[/list]")
								.Replace("[/list:o]", "[/list]")) ?? "");

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
						Subject = decodedSubject,
						Text = fixedText,
						EnableBbCode = enableBbCode,
						EnableHtml = enableHtml,
						CreateTimestamp = ImportHelper.UnixTimeStampToDateTime(p.Timestamp),
						LastUpdateTimestamp = ImportHelper.UnixTimeStampToDateTime(p.LastUpdateTimestamp
							?? p.Timestamp),
						CreateUserName = !string.IsNullOrWhiteSpace(p.PosterName) ? p.PosterName : "Unknown",
						LastUpdateUserName = !string.IsNullOrWhiteSpace(p.LastUpdateUserName)
							? p.LastUpdateUserName
							: !string.IsNullOrWhiteSpace(p.PosterName)
								? p.PosterName
								: "Unknown",
						PosterMood = (ForumPostMood)moodAvatar,
						ForumId = p.ForumId
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
				nameof(ForumPost.CreateTimestamp),
				nameof(ForumPost.CreateUserName),
				nameof(ForumPost.LastUpdateTimestamp),
				nameof(ForumPost.LastUpdateUserName),
				nameof(ForumPost.EnableHtml),
				nameof(ForumPost.EnableBbCode),
				nameof(ForumPost.PosterMood),
				nameof(ForumPost.ForumId)
			};

			posts.BulkInsert(columns, nameof(ApplicationDbContext.ForumPosts));
		}
	}
}
