﻿namespace TASVideos.Data.Entity;

public class PublicationFlag
{
	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }

	public int FlagId { get; set; }
	public Flag? Flag { get; set; }
}

public static class PublicationFlagExtensions
{
	public static void AddFlags(this ICollection<PublicationFlag> flags, IEnumerable<int> flagIds)
	{
		flags.AddRange(flagIds.Select(t => new PublicationFlag
		{
			FlagId = t
		}));
	}
}
