using System;

namespace TASVideos.Core
{
	/// <summary>
	/// Indicates that a collection can be sorted by this property
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class SortableAttribute : Attribute
	{
	}

	/// <summary>
	/// Indicates that a property should not be considered when building a dynamic table
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class TableIgnoreAttribute : Attribute
	{
	}

	/// <summary>
	/// Indicates that a property should be hidden in mobile widths when building a dynamic table.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MobileHideAttribute : Attribute
	{
	}
}
