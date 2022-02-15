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

	[Display(Name = "Player Rank")]
	public string Rank { get; set; } = "";
}

public class PublicationPointsModel
{
	[Display(Name = "Pos")]
	public int Position { get; set; } = 0;

	[Display(Name = "Movie Id")]
	public int Id { get; init; } = 0;

	[Display(Name = "Movie")]
	public string Title { get; init; } = "";

	[Display(Name = "Points")]
	public double Points { get; set; } = 0.0;
}
