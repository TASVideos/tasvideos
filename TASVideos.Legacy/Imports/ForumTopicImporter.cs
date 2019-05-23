using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy.Data.Forum.Entity
{
    public static class ForumTopicImporter
    {
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var legTopics = legacyForumContext.Topics
				.Include(t => t.Poll)
				.Where(t => t.TopicMovedId == 0) // Topic moved id indicates it is not a valid topic anymore, so simply filter them out
				.Select(t => new
				{
					t.Id,
					t.ForumId,
					t.Title,
					t.PosterId,
					t.Timestamp,
					t.Views,
					t.Type,
					t.TopicStatus,
					Author = t.PosterId > 0 ? t.Poster.UserName : "Unknown",
					PollId = t.Poll != null ? t.Poll.Id : (int?)null,
					PageName = t.SubmissionId > 0 ? "InternalSystem/SubmissionContent/S" + t.SubmissionId : null
				})
				.ToList();

			 var topics = legTopics
				.Select(t => new ForumTopic
				{
					Id = t.Id,
					ForumId = t.ForumId,
					Title = WebUtility.HtmlDecode(ImportHelper.ConvertLatin1String(t.Title)),
					PosterId = t.PosterId > 0 // There's one record that is 0 we want to change to -1
						? t.PosterId.Value
						: -1,
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					CreateUserName = t.Author,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					LastUpdateUserName = "LegacyImport",
					Views = t.Views,
					Type = (ForumTopicType)t.Type,
					PollId = t.PollId,
					IsLocked = t.TopicStatus == 1,
					PageName = t.PageName
				})
				.ToList();

			var columns = new[]
			{
				nameof(ForumTopic.Id),
				nameof(ForumTopic.ForumId),
				nameof(ForumTopic.Title),
				nameof(ForumTopic.PosterId),
				nameof(ForumTopic.CreateTimeStamp),
				nameof(ForumTopic.LastUpdateTimeStamp),
				nameof(ForumTopic.CreateUserName),
				nameof(ForumTopic.LastUpdateUserName),
				nameof(ForumTopic.Views),
				nameof(ForumTopic.Type),
				nameof(ForumTopic.PollId),
				nameof(ForumTopic.IsLocked),
				nameof(ForumTopic.PageName)
			};

			topics.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.ForumTopics));
		}
	}
}
