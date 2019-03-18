using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace TASVideos.Extensions
{
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Returns the <seealso cref="DescriptionAttribute"/> of an Enum if it exists
		/// Else it will return and empty string
		/// If the value is null, an empty string will be returned
		/// </summary>
		public static string Description(this Enum enumValue)
		{
			var descriptionAttribute = enumValue?.GetType()
				.GetMember(enumValue.ToString())
				.Single()
				.GetCustomAttribute<DescriptionAttribute>();

			if (descriptionAttribute != null)
			{
				return descriptionAttribute.Description;
			}

			var displayAttribute = enumValue?.GetType()
				.GetMember(enumValue.ToString())
				.Single()
				.GetCustomAttribute<DisplayAttribute>();

			if (displayAttribute != null)
			{
				return displayAttribute.Description ?? string.Empty;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns the DisplayAttribute value of a Enum if exists
		/// Else it will return the name of the enum
		/// If the value is null, an empty string will be returned
		/// </summary>
		public static string EnumDisplayName(this Enum enumValue)
		{
			if (enumValue == null)
			{
				return string.Empty;
			}

			var displayName = enumValue
				.GetType()
				.GetMember(enumValue.ToString())
				.Single()
				.GetCustomAttribute<DisplayAttribute>();

			if (displayName != null)
			{
				return displayName.GetName();
			}

			return enumValue.ToString();
		}
	}
}
