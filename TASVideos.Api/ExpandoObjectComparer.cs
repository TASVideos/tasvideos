using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TASVideos.Api
{
	internal class ExpandoObjectComparer : IEqualityComparer<ExpandoObject>
	{
		private ExpandoObjectComparer()
		{
		}

		public static ExpandoObjectComparer Default()
		{
			return new ExpandoObjectComparer();
		}

		public bool Equals(ExpandoObject x, ExpandoObject y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			var xKeyValues = (IDictionary<string, object>)x;
			var yKeyValues = (IDictionary<string, object>)y;

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
				var xValueItem = keyValue.Value;
				var yValueItem = yKeyValues[key];

				if (xValueItem == null & yValueItem != null)
				{
					return false;
				}

				if (xValueItem != null & yValueItem == null)
				{
					return false;
				}

				if (xValueItem != null & yValueItem != null)
				{
					if (!xValueItem.Equals(yValueItem))
					{
						return false;
					}
				}
			}

			return true;
		}


		public int GetHashCode(ExpandoObject obj)
		{
			int hashCode = 0;

			int GetHash(object item)
			{
				if (item == null)
				{
					return 0;
				}

				return item.GetHashCode();
			}

			var fieldValues = new Dictionary<string, object>(obj);
			fieldValues.Values.ToList().ForEach(v => hashCode ^= GetHash(v));
			return hashCode;
		}
	}
}
