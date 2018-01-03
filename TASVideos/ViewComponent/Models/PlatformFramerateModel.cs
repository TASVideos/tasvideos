using System.ComponentModel.DataAnnotations;

namespace TASVideos.ViewComponents
{
    public class PlatformFramerateModel
    {
		[Display(Name = "System")]
		public string SystemCode { get; set; }

		[Display(Name = "Region")]
		public string RegionCode { get; set; }

		[Display(Name = "Framerate")]
		public double FrameRate { get; set; }

		[Display(Name = "Preliminary or approximate")]
		public bool Preliminary { get; set; }
    }
}
