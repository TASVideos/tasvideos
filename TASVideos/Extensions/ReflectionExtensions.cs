using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TASVideos.Attributes;

namespace TASVideos.Extensions
{
    public static class ReflectionExtensions
    {
		/// <summary>
		/// Retruns the <seealso cref="DescriptionAttribute"/> of an Enum if it exists
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

			return string.Empty;
		}

		/// <summary>
		/// Retruns the <seealso cref="GroupAttribute"/> of an Enum if it exists
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
	}
}
