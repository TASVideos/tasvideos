using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumPollOption : BaseEntity
	{
		public int Id { get; set; }

		[Required]
		public string Text { get; set; }

		[Required]
		public int Ordinal { get; set; }

		public int PollId { get; set; }
		public virtual ForumPoll Poll { get; set; }

		public virtual ICollection<ForumPollOptionVote> Votes { get; set; } = new HashSet<ForumPollOptionVote>();
	}

	public static class ForumPollOptionExtensions
	{
		public static IQueryable<ForumPollOption> ForPoll(this IQueryable<ForumPollOption> list, int pollId)
		{
			return list.Where(o => o.PollId == pollId);
		}
	}
}
