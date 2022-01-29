﻿using System;
using System.Linq;
using System.Reflection;
using TASVideos.Extensions;

namespace TASVideos.Core;

/// <summary>
/// Represents a request for a data collection that can be sorted.
/// </summary>
public interface ISortable
{
	/// <summary>
	/// Gets a comma separated list of fields to order by
	/// -/+ can be used to denote descending/ascending sort
	/// The default is ascending sort.
	/// </summary>
	string? Sort { get; }
}

public static class SortableExtensions
{
	/// <summary>
	/// Returns whether or not the given parameter is specified.
	/// </summary>
	public static bool IsSortingParam(this ISortable? sortable, string? param)
	{
		if (string.IsNullOrWhiteSpace(sortable?.Sort))
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(param))
		{
			return false;
		}

		return sortable.Sort
			.SplitWithEmpty(",")
			.Select(str => str.Trim())
			.Any(str => string.Equals(
				str.Replace("-", "").Replace("+", ""),
				param,
				StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Returns whether or not the given parameter is specified and
	/// is specified as a descending sort.
	/// </summary>
	public static bool IsDescending(this ISortable? sortable, string? param)
	{
		if (string.IsNullOrWhiteSpace(sortable?.Sort))
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(param))
		{
			return false;
		}

		return sortable.Sort
			.SplitWithEmpty(",")
			.Select(str => str.Trim())
			.Any(str => string.Equals(
					str.Replace("-", "").Replace("+", ""),
					param,
					StringComparison.OrdinalIgnoreCase)
				&& str.StartsWith("-"));
	}

	/// <summary>
	/// Returns whether or not the requested sort is valid based on the destination response
	/// The sorting is valid if all parameters match properties in the response, and that
	/// those properties are declared as sortable.
	/// </summary>
	public static bool IsValidSort(this ISortable? request, Type? response)
	{
		if (response == null)
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(request?.Sort))
		{
			return true;
		}

		var requestedSorts = request.Sort
			.SplitWithEmpty(",")
			.Select(str => str.Trim())
			.Select(s => s.Replace("-", ""))
			.Select(s => s.Replace("+", ""))
			.Select(s => s.ToLower());

		var sortableProperties = response
			.GetProperties()
			.Where(p => p.GetCustomAttribute<SortableAttribute>() != null)
			.Select(p => p.Name.ToLower())
			.ToList();

		return requestedSorts.All(s => sortableProperties.Contains(s));
	}
}
