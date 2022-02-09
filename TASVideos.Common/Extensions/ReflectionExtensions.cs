using System.Collections;
using System.ComponentModel;
using System.Reflection;

using TASVideos.Attributes;

namespace TASVideos.Extensions;

public static class ReflectionExtensions
{
	/// <summary>
	/// Returns the value of the given property, in a UI consumable way
	/// For example, collections will be turned into a pipe separated list.
	/// </summary>
	public static string ToValue(this PropertyInfo property, object? obj)
	{
		if (obj == null)
		{
			return "";
		}

		if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
			&& property.PropertyType.IsGenericType)
		{
			var val = property.GetValue(obj);
			if (val == null)
			{
				return "";
			}

			var values = ((IEnumerable)val).Cast<object>();
			return string.Join("|", values);
		}

		return property.GetValue(obj)?.ToString() ?? "";
	}

	/// <summary>
	/// Returns the <seealso cref="GroupAttribute"/> of an Enum if it exists
	/// Else it will return and empty string
	/// If the value is null, an empty string will be returned.
	/// </summary>
	public static string Group(this Enum? enumValue)
	{
		var descriptionAttribute = enumValue?.GetType()
			.GetMember(enumValue.ToString())
			.Single()
			.GetCustomAttribute<GroupAttribute>();

		return descriptionAttribute is not null
			? descriptionAttribute.Name
			: string.Empty;
	}

	public static string DisplayName(this PropertyInfo propertyInfo)
	{
		var displayAttr = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
		return displayAttr is not null
			? displayAttr.DisplayName
			: propertyInfo.Name;
	}
}
