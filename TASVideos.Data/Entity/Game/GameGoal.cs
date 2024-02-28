namespace TASVideos.Data.Entity.Game;

public class GameGoal
{
	public int Id { get; set; }
	public int GameId { get; set; }
	public virtual Game? Game { get; set; }

	[StringLength(50)]
	public string DisplayName { get; set; } = "";

	public virtual ICollection<Publication> Publications { get; set; } = new HashSet<Publication>();
	public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
}
