using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using TASVideos.Attributes;

namespace TASVideos.Extensions
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

		/// <summary>
		/// Returns the <seealso cref="GroupAttribute"/> of an Enum if it exists
		/// Else it will return and empty string
		/// If the value is null, an empty string will be returned
		/// </summary>
		public static string Group(this Enum enumValue)
		{
			var descriptionAttribute = enumValue?.GetType()
				.GetMember(enumValue.ToString())
				.Single()
				.GetCustomAttribute<GroupAttribute>();

			if (descriptionAttribute != null)
			{
				return descriptionAttribute.Name;
			}

			return string.Empty;
		}

		public static string DisplayName(this PropertyInfo propertyInfo)
		{
			var displayAttr = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
			if (displayAttr != null)
			{
				return displayAttr.DisplayName;
			}

			return propertyInfo.Name;
		}
	}
}
