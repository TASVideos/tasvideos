using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Awards
{
	public enum AwardType
	{
		User = 1,
		Movie
	}

	public class Award
	{
		public int Id { get; set; }
		public AwardType Type { get; set; }

		[Required]
		[StringLength(25)]
		public string ShortName { get; set; }

		[Required]
		[StringLength(50)]
		public string Description { get; set; }
	}
}
