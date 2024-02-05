using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASVideos.Data.Entity.Exhibition;

public enum FileType
{
	Screenshot, MovieFile
}

public class ExhibitionFile
{
	public int Id { get; set; }
	public int ExhibitionId { get; set; }
	public virtual Exhibition? Exhibition { get; set; }

	public string Path { get; set; } = "";
	public FileType Type { get; set; }
	public string? Description { get; set; }
	public byte[]? FileData { get; set; }
}
