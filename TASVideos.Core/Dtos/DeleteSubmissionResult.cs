namespace TASVideos.Core.Services;

public record DeleteSubmissionResult(
	DeleteSubmissionResult.DeleteStatus Status,
	string SubmissionTitle,
	string ErrorMessage)
{
	public enum DeleteStatus { Success, NotFound, NotAllowed }

	public bool True => Status == DeleteStatus.Success;

	internal static DeleteSubmissionResult NotFound() => new(DeleteStatus.NotFound, "", "");

	internal static DeleteSubmissionResult IsPublished(string submissionTitle) => new(
		DeleteStatus.NotAllowed,
		submissionTitle,
		"Cannot delete a submission that is published");

	internal static DeleteSubmissionResult Success(string submissionTitle)
		=> new(DeleteStatus.Success, submissionTitle, "");
}
