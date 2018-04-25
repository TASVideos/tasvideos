using System;
using System.Data.SqlClient;
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
			/******** ForumPoll ********/
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

			var pollColumns = new[]
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

			forumPolls.BulkInsert(context, pollColumns, nameof(ApplicationDbContext.ForumPolls));

			/******** ForumPollOption ********/
			var forumPollOptions = (from vr in legacyForumContext.VoteResult
				join v in legacyForumContext.VoteDescription on vr.Id equals v.Id // The joins are to filter out orphan options
				join t in legacyForumContext.Topics on v.TopicId equals t.Id 
				select vr)
				.Select(r => new ForumPollOption
				{
					Text = r.VoteOptionText,
					PollId = r.Id,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow,
					CreateUserName = "Unknown", // TODO: could use the topic creator
					LastUpdateUserName = "Unknown"
				})
				.ToList();

			var pollOptionColumns = new[]
			{
				nameof(ForumPollOption.Text),
				nameof(ForumPollOption.PollId),
				nameof(ForumPollOption.CreateTimeStamp),
				nameof(ForumPollOption.CreateUserName),
				nameof(ForumPollOption.LastUpdateTimeStamp),
				nameof(ForumPollOption.LastUpdateUserName)
			};

			forumPollOptions.BulkInsert(context, pollOptionColumns, nameof(ApplicationDbContext.ForumPollOptions), SqlBulkCopyOptions.Default);

			/******** ForumPollOptionVote ********/
			var forumVoters = legacyForumContext.VoteResult.ToList();
		}
	}
}
