using System.Reflection;

namespace TASVideos.Core;

public interface IRequest : IPageable, ISortable
{
}

public interface IPaged<out T>
	where T : IRequest
{
	int RowCount { get; }
	T Request { get; }
}

public static class RequestExtensions
{
	public static IDictionary<string, string> AdditionalProperties(this IRequest? request)
	{
		if (request is null)
		{
			return new Dictionary<string, string>();
		}

		var existing = typeof(IRequest)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Concat(typeof(IRequest)
				.GetInterfaces()
				.SelectMany(i => i.GetProperties()))
			.ToList();

		var existingNames = existing.Select(p => p.Name);

		var all = request
			.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.ToList();

		var additional = all
			.Where(p => !existingNames.Contains(p.Name))
			.ToList();

		return additional.ToDictionary(tkey => tkey.Name, tvalue => tvalue.ToValue(request));
	}
}

public static class PagedExtensions
{
	public static int LastPage<T>(this IPaged<T>? paged)
		where T : IRequest
	{
		var size = paged?.Request.PageSize ?? 0;
		var count = paged?.RowCount ?? 0;
		if (count <= 0 || size <= 0)
		{
			return 0;
		}

		return (int)Math.Ceiling(count / (double)size);
	}

	public static int LastRow<T>(this IPaged<T>? paged)
		where T : IRequest
	{
		var size = paged?.Request.PageSize ?? 0;
		var rowCount = paged?.RowCount ?? 0;
		T? request = paged is null ? default : paged.Request;
		return Math.Min(rowCount, request.Offset() + size);
	}
}
