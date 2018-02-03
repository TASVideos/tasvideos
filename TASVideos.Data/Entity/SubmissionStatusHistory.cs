namespace TASVideos.Data.Entity
{
    public class SubmissionStatusHistory : BaseEntity
    {
		public int Id { get; set; }
		public int SubmissionId { get; set; }
		public virtual Submission Submission { get; set; }

		public SubmissionStatus Status { get; set; }
	}
}
