using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents a request object for which an API endpoint collection
	/// that can be sorted, paged, and field selected
	/// </summary>
	public interface IRequestable
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
		/// Gets a comma separated list of fields to order by
		/// -/+ can be used to denote descending/ascending sort
		/// The default is ascending sort
		/// </summary>
		string Sort { get; }

		/// <summary>
		/// Gets a comma separated string that specifies which fields to return in the result set
		/// </summary>
		string Fields { get; }
	}

	/// <summary>
	/// Represents a standard api request
	/// Supports sorting, paging, and field selection parameters
	/// </summary>
	public class ApiRequest : IRequestable
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
				.Distinct(); // TODO: this doesn't work, distinct by equality
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

		/// <summary>
		/// Orders the given collection based on the <see cref="IRequestable.Sort"/> property
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IRequestable adj)
		{
			// TODO: non-sortable columns
			if (string.IsNullOrWhiteSpace(adj?.Sort))
			{
				return source;
			}

			var columns = adj.Sort.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			bool thenBy = false;
			foreach (var column in columns)
			{
				source = SortByParam(source, column, thenBy);
				thenBy = true;
			}

			return source;
		}

		private static IQueryable<T> SortByParam<T>(IQueryable<T> query, string column, bool thenBy)
		{
			bool desc = column.StartsWith("-");

			column = column.Trim('-').Trim('+')?.ToLower();

			var hasProperty = typeof(T).GetProperties().Any(p => p.Name.ToLower() == column);

			if (!hasProperty)
			{
				return query;
			}

			string orderBy;
			if (thenBy)
			{
				orderBy = desc
					? nameof(Enumerable.ThenByDescending)
					: nameof(Enumerable.ThenBy);
			}
			else
			{
				orderBy = desc
					? nameof(Enumerable.OrderByDescending)
					: nameof(Enumerable.OrderBy);
			}

			// https://stackoverflow.com/questions/34899933/sorting-using-property-name-as-string
			// LAMBDA: x => x.[PropertyName]
			var parameter = Expression.Parameter(typeof(T), "x");
			Expression property = Expression.Property(parameter, column);
			var lambda = Expression.Lambda(property, parameter);

			// REFLECTION: source.OrderBy(x => x.Property)
			var orderByMethod = typeof(Queryable).GetMethods().First(x => x.Name == orderBy && x.GetParameters().Length == 2);
			var orderByGeneric = orderByMethod.MakeGenericMethod(typeof(T), property.Type);
			var result = orderByGeneric.Invoke(null, new object[] { query, lambda });

			return (IQueryable<T>)result;
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
