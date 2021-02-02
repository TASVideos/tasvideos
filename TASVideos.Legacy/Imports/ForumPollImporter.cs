﻿using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPollImporter
	{
		public static void
			Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var legacyVoteDescriptions = legacyForumContext.VoteDescription
				.Include(v => v.Topic)
				.ThenInclude(t => t!.Poster)
				.Include(v => v.Results)
				.Where(v => v.Topic != null && v.Topic.TopicMovedId == 0)
				.ToList();

			/******** ForumPoll ********/
			var forumPolls = legacyVoteDescriptions
				.Select(v => new ForumPoll
				{
					Id = v.Id,
					TopicId = v.TopicId,
					Question = SwapSubmissionPoll(ImportHelper.ConvertNotNullLatin1String(v.Text), v.Topic?.Poster?.UserName),
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(v.VoteStart),
					CreateUserName = v.Topic?.Poster?.UserName ?? "Unknown",
					LastUpdateUserName = v.Topic?.Poster?.UserName ?? "Unknown",
					CloseDate = v.VoteLength == 0
						? (DateTime?)null
						: ImportHelper.UnixTimeStampToDateTime(v.VoteStart + v.VoteLength)
				})
				.ToList();

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

			forumPolls.BulkInsert(connectionStr, pollColumns, nameof(ApplicationDbContext.ForumPolls));

			/******** ForumPollOption ********/
			var legForumPollOptions = legacyVoteDescriptions
				.SelectMany(vd => vd.Results)
				.ToList();

			var forumPollOptions = legForumPollOptions
			.Select(r => new ForumPollOption
				{
					Text = r.VoteOptionText,
					PollId = r.Id,
					Ordinal = r.VoteOptionId,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow,
					CreateUserName = r.VoteDescription?.Topic?.Poster?.UserName ?? "Unknown",
					LastUpdateUserName = r.VoteDescription?.Topic?.Poster?.UserName ?? "Unknown"
				})
				.ToList();

			var pollOptionColumns = new[]
			{
				nameof(ForumPollOption.Text),
				nameof(ForumPollOption.Ordinal),
				nameof(ForumPollOption.PollId),
				nameof(ForumPollOption.CreateTimeStamp),
				nameof(ForumPollOption.CreateUserName),
				nameof(ForumPollOption.LastUpdateTimeStamp),
				nameof(ForumPollOption.LastUpdateUserName)
			};

			forumPollOptions.BulkInsert(connectionStr, pollOptionColumns, nameof(ApplicationDbContext.ForumPollOptions), SqlBulkCopyOptions.Default);

			/******** ForumPollOptionVote ********/
			var legForumVoters = legacyForumContext.Voter.ToList();
			var newForumOptions = context.ForumPollOptions.ToList();

			var forumPollOptionVotes =
				(from v in legForumVoters
				join po in newForumOptions on new { PollId = v.Id, Ordinal = v.OptionId } equals new { po.PollId, po.Ordinal }
				select new ForumPollOptionVote
				{
					PollOptionId = po.Id,
					UserId = v.UserId,
					IpAddress = v.IpAddress,
					CreateTimestamp = DateTime.UtcNow // Legacy system did not track this
				})
				.ToList();

			// Insert Unknown User votes for discrepancies between de-normalized vote count and actual vote records
			var missingVotes =
				(from po in legForumPollOptions
				 join v in legForumVoters on new { po.Id, OptionId = po.VoteOptionId } equals new { v.Id, v.OptionId }
				 join newPo in newForumOptions on new { PollId = v.Id, Ordinal = po.VoteOptionId } equals new { newPo.PollId, newPo.Ordinal }
				 select new { po, v, newPo })
				.GroupBy(tkey => new { tkey.newPo.Id, tkey.po.ResultCount })
				.Select(g => new { g.Key.Id, g.Key.ResultCount, Actual = g.Count() })
				.Where(x => x.ResultCount > x.Actual)
				.ToList();

			foreach (var option in missingVotes)
			{
				var diff = option.ResultCount - option.Actual;

				for (int i = 0; i < diff; i++)
				{
					forumPollOptionVotes.Add(new ForumPollOptionVote
					{
						PollOptionId = option.Id,
						UserId = -1,
						IpAddress = null,
						CreateTimestamp = DateTime.UtcNow
					});
				}
			}

			var pollVoteColumns = new[]
			{
				nameof(ForumPollOptionVote.PollOptionId),
				nameof(ForumPollOptionVote.UserId),
				nameof(ForumPollOptionVote.CreateTimestamp),
				nameof(ForumPollOptionVote.IpAddress)
			};

			forumPollOptionVotes.BulkInsert(connectionStr, pollVoteColumns, nameof(ApplicationDbContext.ForumPollOptionVotes), SqlBulkCopyOptions.Default);
		}

		// Removes html silliness from the poll questions
		private static string SwapSubmissionPoll(string pollText, string? user)
		{
			// the latter user was an April Fool's gag
			if ((user == SiteGlobalConstants.TASVideoAgent || user == "poozilla") && pollText.StartsWith("Vote: "))
			{
				if (pollText.StartsWith("Vote: Did you like watching this movie?"))
				{
					return "Vote: Did you like watching this movie? (Vote after watching!)";
				}

				if (pollText.StartsWith("Vote: Should this movie be published?"))
				{
					return "Vote: Should this movie be published? (Vote after watching!)";
				}

				return "Vote: Did you find this movie entertaining? (Vote after watching!)";
			}

			return pollText;
		}
	}
}
