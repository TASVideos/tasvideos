using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface ISubmissionDisplay
{
	SubmissionStatus Status { get; }
	DateTime Submitted { get; }
}