namespace TASVideos.Data.Entity.Forum
{
	public class ForumPrivateMessage : BaseEntity
	{
		public int Id { get; set; }

		public int FromUserId { get; set; }
		public virtual User FromUser { get; set; }

		public int ToUserId { get; set; }
		public virtual User ToUser { get; set; }

		public string IpAddress { get; set; }

		public string Subject { get; set; }
		public string Text { get; set; }

		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }
	}
}
