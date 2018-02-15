using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class MovieFile
    {
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("movieid")]
		public int MovieId { get; set; }

		[Column("filename")]
		public string FileName { get; set; }

		[Column("filererecords")]
		public int FileRerecords { get; set; }

		[Column("typech")]
		public string Type { get; set; }

		[Column("description")]
		public string Description { get; set; }

		[Column("title")]
		public string Title { get; set; }
	}
}
