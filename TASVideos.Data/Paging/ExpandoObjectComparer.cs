using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TASVideos.Data
{
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

			if (x == null || y == null) // ReferenceEquals checks the scenario of both being null
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
			if (missingKey != null)
			{
				return false;
			}

			foreach (var keyValue in xKeyValues)
			{
				var key = keyValue.Key;
				var xValueItem = keyValue.Value ?? new object();
				var yValueItem = yKeyValues[key];

				if (yValueItem == null)
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
}
