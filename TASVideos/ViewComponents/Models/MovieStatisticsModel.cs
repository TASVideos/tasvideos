using System.Globalization;
using TASVideos.Common;

namespace TASVideos.ViewComponents;

public class MovieStatisticsModel
{
	public string ErrorMessage { get; init; } = "";
	public string FieldHeader { get; init; } = "";
	public IReadOnlyCollection<MovieStatisticsEntry> MovieList { get; init; } = [];

	public class MovieStatisticsEntry
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public object Value { get; init; } = new();

		public string? DisplayString()
		{
			if (Value is TimeSpan t)
			{
				return t.ToStringWithOptionalDaysAndHours();
			}

			if (Value is double f)
			{
				return f.ToString(CultureInfo.CurrentCulture);
			}

			return Value.ToString();
		}
	}
}
