namespace TASVideos.Data.Entity.Exhibition;
public enum ExhibitionUrlType { Streaming, External }
public class ExhibitionUrl
{
	public int Id { get; set; }
	public int ExhibitionId { get; set; }
	public virtual Exhibition? Exhibition { get; set; }

	public string Url { get; set; } = "";
	public ExhibitionUrlType Type { get; set; }
	public string? DisplayName { get; set; }
}
