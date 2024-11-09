namespace TASVideos.Data.Entity.Forum;

[ExcludeFromHistory]
public class ForumPoll : BaseEntity
{
	public int Id { get; set; }

	public int TopicId { get; set; }
	public ForumTopic? Topic { get; set; }

	[StringLength(500)]
	public string Question { get; set; } = "";

	public DateTime? CloseDate { get; set; }

	public bool MultiSelect { get; set; }

	public ICollection<ForumPollOption> PollOptions { get; set; } = [];
}
