namespace TASVideos.Data.Entity.Game;

public class Goal : BaseEntity
{
	public int Id { get; set; }

	[Required]
	[StringLength(50)]
	public string DisplayName { get; set; } = "";

	public virtual ICollection<GameGoal> GameGoals { get; set; } = new HashSet<GameGoal>();
}
