using System;

namespace TASVideos.RazorPages.Pages.Activity.Model
{
	public class ActivitySummaryModel
	{
		public string? UserName { get; set; }
		public int Count { get; set; }
		public DateTime LastActivity { get; set; }
	}
}
