using System;
using System.Collections.Generic;

using TASVideos.Data;

#pragma warning disable 1591
namespace TASVideos.Api.Responses
{
	/// <summary>
	/// Represents a publication returned by the publications endpoint
	/// </summary>
	public class PublicationsResponse
	{
		[Sortable]
		public int Id { get; set; }

		[Sortable]
		public string Title { get; set; }

		[Sortable]
		public string Branch { get; set; }

		[Sortable]
		public string EmulatorVersion { get; set; }

		[Sortable]
		public string Tier { get; set; }

		[Sortable]
		public string SystemCode { get; set; }

		[Sortable]
		public int SubmissionId { get; set; }

		[Sortable]
		public int GameId { get; set; }

		[Sortable]
		public int RomId { get; set; }

		[Sortable]
		public int ObsoletedById { get; set; }

		[Sortable]
		public int Frames { get; set; }

		[Sortable]
		public int RerecordCount { get; set; }

		[Sortable]
		public double SystemFrameRate { get; set; }

		[Sortable]
		public string MovieFileName { get; set; }

		public IEnumerable<string> Authors { get; set; } = new string[0];
		public IEnumerable<string> Tags { get; set; } = new string[0];
		public IEnumerable<string> Flags { get; set; } = new string[0];
	}
}
