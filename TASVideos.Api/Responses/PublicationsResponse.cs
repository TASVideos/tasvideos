using System;
using System.Collections.Generic;
using TASVideos.Data;

#pragma warning disable 1591
namespace TASVideos.Api.Responses
{
	/// <summary>
	/// Represents a publication returned by the publications endpoint.
	/// </summary>
	public class PublicationsResponse
	{
		[Sortable]
		public int Id { get; init; }

		[Sortable]
		public string Title { get; init; } = "";

		[Sortable]
		public string Branch { get; init; } = "";

		[Sortable]
		public string EmulatorVersion { get; init; } = "";

		[Sortable]
		public string Tier { get; init; } = "";

		[Sortable]
		public string SystemCode { get; init; } = "";

		[Sortable]
		public int SubmissionId { get; init; }

		[Sortable]
		public int GameId { get; init; }

		[Sortable]
		public int RomId { get; init; }

		[Sortable]
		public int ObsoletedById { get; init; }

		[Sortable]
		public int Frames { get; init; }

		[Sortable]
		public int RerecordCount { get; init; }

		[Sortable]
		public double SystemFrameRate { get; init; }

		[Sortable]
		public string MovieFileName { get; init; } = "";

		public IEnumerable<string> Authors { get; init; } = Array.Empty<string>();
		public IEnumerable<string> Tags { get; init; } = Array.Empty<string>();
		public IEnumerable<string> Flags { get; init; } = Array.Empty<string>();
	}
}
