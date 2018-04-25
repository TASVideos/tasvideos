using System;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPollImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var legVoteDescriptions = (from p in legacyForumContext.VoteDescription
				join t in legacyForumContext.Topics on p.TopicId equals t.Id // The join is to filter out orphan polls
				select p)
				.ToList();

			var forumPolls = legVoteDescriptions
				.Select(v => new ForumPoll
				{
					Id = v.Id,
					TopicId = v.TopicId,
					Question = ImportHelper.FixString(v.Text),
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(v.VoteStart),
					CreateUserName = "Unknown", // TODO: we could try to get the topic creator when not -1 if it is worth it
					LastUpdateUserName = "Unknown", // Ditto
					CloseDate = v.VoteLength == 0
						? (DateTime?)null
						: ImportHelper.UnixTimeStampToDateTime(v.VoteStart + v.VoteLength)
				});

			var columns = new[]
			{
				nameof(ForumPoll.Id),
				nameof(ForumPoll.TopicId),
				nameof(ForumPoll.Question),
				nameof(ForumPoll.CloseDate),
				nameof(ForumPoll.CreateTimeStamp),
				nameof(ForumPoll.CreateUserName),
				nameof(ForumPoll.LastUpdateTimeStamp),
				nameof(ForumPoll.LastUpdateUserName)
			};

			forumPolls.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumPolls));
		}
	}
}
