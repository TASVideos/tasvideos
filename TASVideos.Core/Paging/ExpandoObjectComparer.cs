using System.Dynamic;

namespace TASVideos.Core;

internal class ExpandoObjectComparer : IEqualityComparer<ExpandoObject>
{
	private ExpandoObjectComparer()
	{
	}

	public static ExpandoObjectComparer Default()
	{
		return new();
	}

	public bool Equals(ExpandoObject? x, ExpandoObject? y)
	{
		if (ReferenceEquals(x, y))
		{
			return true;
		}

		// ReferenceEquals checks the scenario of both being null
		if (x is null || y is null)
		{
			return false;
		}

		var xKeyValues = (IDictionary<string, object?>)x;
		var yKeyValues = (IDictionary<string, object?>)y;

		if (xKeyValues.Count != yKeyValues.Count)
		{
			return false;
		}

		var missingKey = xKeyValues.Keys.FirstOrDefault(k => !yKeyValues.ContainsKey(k));
		if (missingKey is not null)
		{
			return false;
		}

		foreach ((var key, object? value) in xKeyValues)
		{
			var xValueItem = value ?? new object();
			var yValueItem = yKeyValues[key];

			if (yValueItem is null)
			{
				return false;
			}

			if (!xValueItem.Equals(yValueItem))
			{
				return false;
			}
		}

		return true;
	}

	public int GetHashCode(ExpandoObject obj)
	{
		int hashCode = 0;

		static int GetHash(object item)
		{
			return item.GetHashCode();
		}

		var fieldValues = new Dictionary<string, object?>(obj);
		fieldValues.Values.ToList().ForEach(v => hashCode ^= GetHash(v ?? new object()));
		return hashCode;
	}
}
