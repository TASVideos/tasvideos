using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TASVideos.Data
{
	/// <summary>
	/// Represents a request object that can have field selection.
	/// Field selection restricts the values returned only to those requests
	/// </summary>
	public interface IFieldSelectable
	{
		/// <summary>
		/// Gets a comma separated string that specifies which fields to return in the result set
		/// </summary>
		string Fields { get; }
	}

	public static class FieldSelectionExtensions
	{
		/// <summary>
		/// Returns a list of objects that only contains the properties from the
		/// <see cref="IFieldSelectable.Fields"/> column
		/// properties are specified, all the properties are returned
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		public static IEnumerable<ExpandoObject> FieldSelect<T>(this IEnumerable<T> source, IFieldSelectable fields)
		{
			if (string.IsNullOrWhiteSpace(fields?.Fields))
			{
				return source.Select(s => s.FieldSelect(""));
			}

			return source
				.Select(s => s.FieldSelect(fields?.Fields))
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
