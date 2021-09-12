using System;
using System.Collections.Generic;
using TASVideos.Common;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class MovieStatisticsModel
	{
		public string ErrorMessage { get; init; } = "";
		public string FieldHeader { get; init; } = "";
		public ICollection<MovieStatisticsEntry> MovieList { get; init; } = new List<MovieStatisticsEntry>();

		public class MovieStatisticsEntry
		{
			public int Id { get; init; }
			public string Title { get; init; } = "";
			public virtual string DisplayValue => Id.ToString();
			public virtual IComparable Comparable => Id;
		}

		public class MovieStatisticsIntEntry : MovieStatisticsEntry
		{
			public int IntValue { get; init; }
			public override string DisplayValue => IntValue.ToString();
			public override IComparable Comparable => IntValue;
		}

		public class MovieStatisticsFloatEntry : MovieStatisticsEntry
		{
			public float FloatValue { get; init; }
			public override string DisplayValue => FloatValue.ToString();
			public override IComparable Comparable => FloatValue;
		}

		public class MovieStatisticsTimeSpanEntry : MovieStatisticsEntry
		{
			public TimeSpan TimeSpanValue { get; init; }
			public override string DisplayValue => TimeSpanValue.ToStringWithOptionalDaysAndHours();
			public override IComparable Comparable => TimeSpanValue;
		}
	}
}
