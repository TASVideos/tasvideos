using System;

namespace TASVideos.RazorPages.ViewComponents
{
	public class FramesModel
	{
		public double Fps { get; init; }
		public int Amount { get; init; }

		public TimeSpan TimeSpan => TimeSpan.FromSeconds(Amount / Fps);
	}
}
