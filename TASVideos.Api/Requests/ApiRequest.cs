/*
* General API TODOs:
* Field selection is purely post-processing and returns distinct objects,
* so the record count might be less than the requested count
* how do we document this? or do we want to try to do dynamic queryable field selection?
*/
namespace TASVideos.Api.Requests;

/// <summary>
/// Represents a standard API GET request.
/// Supports sorting, paging, and field selection parameters.
/// </summary>
internal class ApiRequest : IFieldSelectable, ISortable, IPageable
{
	[SwaggerParameter("The total number of records to return. If not specified, then a default number of records will be returned")]
	public int? PageSize { get; init; } = 100;

	[SwaggerParameter("The page to start returning records. If not specified, then an offset of 1 will be used")]
	public int? CurrentPage { get; init; } = 1;

	[SwaggerParameter("The fields to sort by. If multiple sort parameters, the list should be comma separated. Precede the parameter with a + or - to sort ascending or descending respectively. If not specified, then a default sort will be used")]
	public string? Sort { get; init; }

	[SwaggerParameter("The fields to return. If multiple, fields must be comma separated. If not specified, then all fields will be returned")]
	public string? Fields { get; init; }

	public const int MaxPageSize = 100;
}

internal static class RequestableExtensions
{
	public static IQueryable<T> SortAndPaginate<T>(this IQueryable<T> source, ApiRequest request)
	{
		int offset = request.Offset();
		int limit = request.PageSize ?? ApiRequest.MaxPageSize;
		return source
			.SortBy(request)
			.Skip(offset)
			.Take(limit);
	}
}
