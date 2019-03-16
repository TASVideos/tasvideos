using TASVideos.Data;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents a request object for which an API endpoint collection
	/// that can be sorted, paged, and field selected
	/// </summary>
	public interface IRequestable : ISortable
	{
		/// <summary>
		/// Gets the max number records to return
		/// </summary>
		int? PageSize { get; }

		/// <summary>
		/// Gets the page to start returning records
		/// </summary>
		int? CurrentPage { get; }

		/// <summary>
		/// Gets a comma separated string that specifies which fields to return in the result set
		/// </summary>
		string Fields { get; }
	}
}
