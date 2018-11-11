namespace TASVideos.Data.Entity.Forum
{
	public class ForumTopicWatch
	{
		public int UserId { get; set; }
		public virtual User User { get; set; }

		public int ForumTopicId { get; set; }
		public virtual ForumTopic ForumTopic { get; set; }

		public bool IsNotified { get; set; }
	}
}
