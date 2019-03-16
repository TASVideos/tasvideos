using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Extension methods to perform sorting, paging, and field selection operations
	/// off the <see cref="IRequestable"/> interface
	/// </summary>
	public static class RequestableExtensions
	{
		/// <summary>
		/// Returns the offset of the given request
		/// </summary>
		public static int Offset(this IRequestable request)
		{
			var current = request.CurrentPage ?? 1;
			var pageSize = request.Limit();
			return ((current < 1 ? 1 : current) - 1) * pageSize;
		}

		/// <summary>
		/// Returns the limit (page size) of the given request
		/// </summary>
		public static int Limit(this IRequestable request)
		{
			return request.PageSize ?? ApiConstants.MaxPageSize;
		}

		/// <summary>
		/// Returns a page of data based on the <see cref="IRequestable.CurrentPage"/>
		/// and <see cref="IRequestable.PageSize"/> properties
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static IQueryable<T> Paginate<T>(this IQueryable<T> source, IRequestable request)
		{
			return source
				.Skip(request.Offset())
				.Take(request.Limit());
		}

		/// <summary>
		/// Returns a list of objects that only contains the properties from the
		/// <see cref="IRequestable.Fields"/> column
		/// properties are specified, all the properties are returned
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static IEnumerable<ExpandoObject> FieldSelect<T>(this IEnumerable<T> source, IRequestable adj)
		{
			return source
				.Select(s => s.FieldSelect(adj?.Fields))
				.Distinct(ExpandoObjectComparer.Default());
		}

		/// <summary>
		/// Receives a single object and performs a fields selection operation 
		/// with the given fields
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static ExpandoObject FieldSelect<T>(this T obj, string fields)
		{
			if (string.IsNullOrWhiteSpace(fields))
			{
				return ToExpando(obj);
			}

			var columns = fields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (columns.All(string.IsNullOrWhiteSpace))
			{
				return ToExpando(obj);
			}

			var expando = new ExpandoObject();
			var dict = (IDictionary<string, object>)expando;

			foreach (var column in columns)
			{
				var property = typeof(T)
					.GetProperties()
					.FirstOrDefault(p => p.Name.ToLower() == column?.ToLower());
				if (property != null)
				{
					dict[property.Name] = property.GetValue(obj);
				}
			}

			return expando;
		}

		private static ExpandoObject ToExpando<T>(this T obj)
		{
			var expando = new ExpandoObject();
			var dictionary = (IDictionary<string, object>)expando;

			foreach (var property in obj.GetType().GetProperties())
			{
				dictionary.Add(property.Name, property.GetValue(obj));
			}

			return expando;
		}
	}
}
