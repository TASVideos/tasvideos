using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1401
namespace TASVideos.Data.SeedData
{
	public class RejectionReasonsSeedData
	{
		public static readonly SubmissionRejectionReason[] RejectionReasons =
		{
			new SubmissionRejectionReason { Id = 1, DisplayName = "Game choice" },
			new SubmissionRejectionReason { Id = 2, DisplayName = "Unapproved hack" },
			new SubmissionRejectionReason { Id = 3, DisplayName = "Version" },
			new SubmissionRejectionReason { Id = 4, DisplayName = "Optimization" },
			new SubmissionRejectionReason { Id = 5, DisplayName = "Entertainment" },
			new SubmissionRejectionReason { Id = 6, DisplayName = "Incomplete" },
			new SubmissionRejectionReason { Id = 7, DisplayName = "Goal" },
			new SubmissionRejectionReason { Id = 8, DisplayName = "Emulation" },
			new SubmissionRejectionReason { Id = 9, DisplayName = "Mode" },
			new SubmissionRejectionReason { Id = 10, DisplayName = "Unauthorized" },
			new SubmissionRejectionReason { Id = 11, DisplayName = "Troll" },
			new SubmissionRejectionReason { Id = 12, DisplayName = "Joke" },
			new SubmissionRejectionReason { Id = 13, DisplayName = "Other" }
		};
	}
}
