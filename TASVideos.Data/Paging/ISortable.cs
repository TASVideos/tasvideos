namespace TASVideos.Data
{
	public interface ISortable
	{
		string SortBy { get; }
		bool SortDescending { get; }
	}
}
