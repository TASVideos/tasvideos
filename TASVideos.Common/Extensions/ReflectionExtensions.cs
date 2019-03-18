using System.Collections;
using System.Linq;
using System.Reflection;

namespace TASVideos.Common.Extensions
{
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Returns the value of the given property, in a UI consumable way
		/// For example, collections will be turned into a pipe separated list
		/// </summary>
		public static string ToValue(this PropertyInfo property, object obj)
		{
			if (obj == null || property == null)
			{
				return null;
			}

			if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
				&& property.PropertyType.IsGenericType)
			{
				var values = ((IEnumerable)property.GetValue(obj)).Cast<object>();
				var val = string.Join("|", values);
				return val;
			}

			return property.GetValue(obj)?.ToString();
		}
	}
}
