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
	public int? PageSize { get; init; } = 100;
	public int? CurrentPage { get; init; } = 1;
	public string? Sort { get; init; }
	public string? Fields { get; init; }

	public const int MaxPageSize = 100;
}

internal static class RequestableExtensions
{
	public static IQueryable<T> Paginate<T>(this IQueryable<T> source, ApiRequest paging)
	{
		int offset = paging.Offset();
		int limit = paging.PageSize ?? ApiRequest.MaxPageSize;
		return source
			.Skip(offset)
			.Take(limit);
	}
}
