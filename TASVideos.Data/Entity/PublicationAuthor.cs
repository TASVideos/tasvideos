namespace TASVideos.Data.Entity;

public class PublicationAuthor
{
	public int UserId { get; set; }
	public User? Author { get; set; }

	public int Ordinal { get; set; }

	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }
}

public static class PublicationAuthorExtensions
{
	public static void CopyFromSubmission(this ICollection<PublicationAuthor> authors, IEnumerable<SubmissionAuthor> subAuthors)
	{
		authors.AddRange(subAuthors
			.Select(sa => new PublicationAuthor
			{
				Author = sa.Author,
				Ordinal = sa.Ordinal
			}));
	}
}
