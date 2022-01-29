using System.ComponentModel.DataAnnotations;

namespace TASVideos.ViewComponents;

public class PlayerPointsModel
{
	[Display(Name = "Pos")]
	public int Position { get; set; } = 0;

	[Display(Name = "PlayerID")]
	public int Id { get; init; } = 0;

	[Display(Name = "Player")]
	public string Player { get; init; } = "";

	[Display(Name = "Points")]
	public double Points { get; set; } = 0.0;
}
