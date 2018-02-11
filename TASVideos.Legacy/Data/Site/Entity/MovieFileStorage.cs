using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TASVideos.Legacy.Data.Site.Entity
{
    public class MovieFileStorage
    {
		[Key]
		[Column("filename")]
		public string FileName { get; set; }

		[Column("filedata")]
		public byte[] FileData { get; set; }

		[Column("filetime")]
		public string FileTime { get; set; }

		[Column("zipped")]
		public string Zipped { get; set; } = "Y";
    }
}
