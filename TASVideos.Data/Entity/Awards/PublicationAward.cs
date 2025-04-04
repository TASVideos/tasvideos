namespace TASVideos.Data.Entity.Awards;

public class PublicationAward
{
	public int Id { get; set; }

	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }

	public int AwardId { get; set; }
	public Award? Award { get; set; }

	public int Year { get; set; }
}
