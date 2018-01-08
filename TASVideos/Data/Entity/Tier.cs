using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
    public class Tier
    {
		public int Id { get; set; }

		[StringLength(20)]
		public string Name { get; set; }
		public double Weight { get; set; }
		public string IconPath { get; set; }

		[Required]
		public string Link { get; set; }
	}
}
