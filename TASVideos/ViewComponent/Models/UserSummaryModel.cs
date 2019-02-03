using System.Collections.Generic;

namespace TASVideos.ViewComponents
{
	public class UserSummaryModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public int EditCount { get; set; }
		public int MovieCount { get; set; }
		public int SubmissionCount { get; set; }
		public IEnumerable<string> Awards { get; set; } = new List<string>();
		public int AwardsWon { get; set; }
	}
}
