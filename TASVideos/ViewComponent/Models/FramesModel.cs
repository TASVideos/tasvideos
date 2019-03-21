using System;

namespace TASVideos.ViewComponents
{
	public class FramesModel
	{
		public double Fps {get; set; }
		public int Amount { get; set; }

		public TimeSpan TimeSpan => TimeSpan.FromSeconds(Amount / Fps);
	}
}
