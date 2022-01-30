namespace TASVideos.Data.Entity.Game;

public class GameRamAddressDomain
{
	public int Id { get; set; }

	[Required]
	[StringLength(255)]
	public string Name { get; set; } = "";

	public int? GameSystemId { get; set; }
	public virtual GameSystem? System { get; set; }
}
