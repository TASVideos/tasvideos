using System.Reflection;
using Microsoft.AspNetCore.Http;

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
public class ApiRequest : IFieldSelectable, ISortable, IPageable
{
	public int? PageSize { get; set; } = 100;
	public int? CurrentPage { get; set; } = 1;
	public string? Sort { get; set; }
	public string? Fields { get; set; }

	public static ValueTask<ApiRequest> BindAsync(HttpContext context, ParameterInfo parameter)
	{
		var result = new ApiRequest
		{
			Sort = context.Request.Query["Sort"],
			Fields = context.Request.Query["Fields"]
		};
		if (int.TryParse(context.Request.Query["PageSize"], out var pageSize))
		{
			result.PageSize = pageSize;
		}

		if (int.TryParse(context.Request.Query["PageSize"], out var currentPage))
		{
			result.CurrentPage = currentPage;
		}

		return ValueTask.FromResult(result);
	}
}

public static class RequestableExtensions
{
	public static IQueryable<T> Paginate<T>(this IQueryable<T> source, ApiRequest paging)
	{
		int offset = paging.Offset();
		int limit = paging.PageSize ?? ApiConstants.MaxPageSize;
		return source
			.Skip(offset)
			.Take(limit);
	}
}
