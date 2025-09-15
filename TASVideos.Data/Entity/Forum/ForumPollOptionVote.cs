namespace TASVideos.Data.Entity.Forum;

public class ForumPollOptionVote
{
	public int Id { get; set; }

	public int PollOptionId { get; set; }
	public ForumPollOption? PollOption { get; set; }

	public int UserId { get; set; }
	public User? User { get; set; }

	public DateTime CreateTimestamp { get; set; }

	public string? IpAddress { get; set; }
}
