using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
    public class Tier
    {
		public int Id { get; set; }

		[Required]
		[StringLength(20)]
		public string Name { get; set; }
		public double Weight { get; set; }

		[Required]
		[StringLength(100)]
		public string IconPath { get; set; }

		[Required]
		[StringLength(100)]
		public string Link { get; set; }
	}
}
