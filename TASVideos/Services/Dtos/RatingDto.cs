namespace TASVideos.Services.Dtos
{
	public class RatingDto
	{
		public double? Overall { get; set; }
		public double? Entertainment { get; set; }
		public double? TechQuality { get; set; }
		public int TotalEntertainmentVotes { get; set; }
		public int TotalTechQualityVotes { get; set; }
	}
}
