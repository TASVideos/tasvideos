using System.ComponentModel.DataAnnotations;
using System.Linq;

using TASVideos.Data;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents a standard api request
	/// Supports sorting, paging, and field selection parameters
	/// </summary>
	public class ApiRequest : IFieldSelectable, ISortable, IPageable
	{
		/// <summary>
		/// Gets or sets the total number of records to return
		/// If not specified then a default number of records will be returned
		/// </summary>
		[Range(1, ApiConstants.MaxPageSize)]
		public int? PageSize { get; set; } = 100;

		/// <summary>
		/// Gets or sets the page to start returning records
		/// If not specified an offset of 1 will be used.
		/// </summary>
		public int? CurrentPage { get; set; } = 1;

		/// <summary>
		/// Gets or sets the fields to sort by.
		/// If multiple sort parameters, the list should be comma separated.
		/// Use - to indicate a descending sort.
		/// A + can optionally be used to indicate an ascending sort.
		/// If not specified then a default sort will be used.
		/// </summary>
		[StringLength(200)]
		public string Sort { get; set; }

		/// <summary>
		/// Gets or sets the fields to return.
		/// If multiple, fields must be comma separated.
		/// If not specified, then all fields will be returned.
		/// </summary>
		[StringLength(200)]
		public string Fields { get; set; }
	}

	/// <summary>
	/// Extension methods to perform sorting, paging, and field selection operations
	/// off the <see cref="ApiRequest"/> class
	/// </summary>
	public static class RequestableExtensions
	{
		/// <summary>
		/// Returns a page of data based on the <see cref="IPageable.CurrentPage"/>
		/// and <see cref="IPageable.PageSize"/> properties
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static IQueryable<T> Paginate<T>(this IQueryable<T> source, ApiRequest paging)
		{
			int offset = paging.GetRowsToSkip();
			int limit = paging.PageSize ?? ApiConstants.MaxPageSize;
			return source
				.Skip(offset)
				.Take(limit);
		}
	}
}
