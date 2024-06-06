namespace TASVideos.Core.Services;

public interface ISubmissionDisplay
{
	SubmissionStatus Status { get; }
	DateTime Date { get; }
}
