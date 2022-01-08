using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public enum FileType
	{
		Screenshot, MovieFile
	}

	public class PublicationFile : BaseEntity
	{
		public int Id { get; set; }

		public int PublicationId { get; set; }
		public virtual Publication? Publication { get; set; }

		[Required]
		[StringLength(250)]
		public string Path { get; set; } = "";
		public FileType Type { get; set; }

		[StringLength(250)]
		public string? Description { get; set; }

		public byte[]? FileData { get; set; }
	}

	public static class PublicationFileExtensions
	{
		public static IQueryable<PublicationFile> ForPublication(this IQueryable<PublicationFile> query, int publicationId)
		{
			return query.Where(pf => pf.PublicationId == publicationId);
		}

		public static IQueryable<PublicationFile> ThatAreMovieFiles(this IQueryable<PublicationFile> query)
		{
			return query.Where(pf => pf.FileData != null);
		}
	}
}
