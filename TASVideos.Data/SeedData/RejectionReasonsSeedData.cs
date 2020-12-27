using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1401
namespace TASVideos.Data.SeedData
{
	public class RejectionReasonsSeedData
	{
		public static readonly SubmissionRejectionReason[] RejectionReasons =
		{
			new () { Id = 1, DisplayName = "Game choice" },
			new () { Id = 2, DisplayName = "Unapproved hack" },
			new () { Id = 3, DisplayName = "Version" },
			new () { Id = 4, DisplayName = "Optimization" },
			new () { Id = 5, DisplayName = "Entertainment" },
			new () { Id = 6, DisplayName = "Incomplete" },
			new () { Id = 7, DisplayName = "Goal" },
			new () { Id = 8, DisplayName = "Emulation" },
			new () { Id = 9, DisplayName = "Mode" },
			new () { Id = 10, DisplayName = "Unauthorized" },
			new () { Id = 11, DisplayName = "Troll" },
			new () { Id = 12, DisplayName = "Joke" },
			new () { Id = 13, DisplayName = "Other" }
		};
	}
}
