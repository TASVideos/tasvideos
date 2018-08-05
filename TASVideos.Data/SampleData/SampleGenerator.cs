using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Data.SampleData
{
	public static class SampleGenerator
	{
		public static readonly Random Rng = new Random(1); // We want random, but deterministic

		public static T AtRandom<T>(this ICollection<T> collection)
		{
			var randomIndex = Rng.Next(0, collection.Count);
			return collection.ElementAtOrDefault(randomIndex);
		}

		public static decimal RandomDecimal(double min, double max, int decimals)
		{
			var randomDecimal = (decimal)((Rng.NextDouble() * (max - min)) + min);
			return Math.Round(randomDecimal, decimals);
		}

		public static bool RandomBool()
		{
			return Rng.NextDouble() >= 0.5;
		}
	}
}
