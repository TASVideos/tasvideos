namespace TASVideos.Data.Entity.Forum
{
    public class ForumPollOptionVote
    {
		public int Id { get; set; }

		public int PollOptionId { get; set; }
		public virtual ForumPollOption PollOption { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }
    }
}
