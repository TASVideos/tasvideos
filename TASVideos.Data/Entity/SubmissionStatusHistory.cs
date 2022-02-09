using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TASVideos.Data.Entity;

public class SubmissionStatusHistory : BaseEntity
{
	public int Id { get; set; }
	public int SubmissionId { get; set; }
	public virtual Submission? Submission { get; set; }

	public SubmissionStatus Status { get; set; }
}

public static class SubmissionStatusHistoryExtensions
{
	public static EntityEntry<SubmissionStatusHistory> Add(this DbSet<SubmissionStatusHistory> history, int submissionId, SubmissionStatus status)
	{
		return history.Add(new SubmissionStatusHistory
		{
			SubmissionId = submissionId,
			Status = status
		});
	}
}
