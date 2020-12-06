namespace TASVideos.Services.Dtos
{
	public class RatingDto
	{
		public double? Overall { get; set; }
		public double? Entertainment { get; init; }
		public double? TechQuality { get; init; }
		public int TotalEntertainmentVotes { get; init; }
		public int TotalTechQualityVotes { get; init; }
	}
}
