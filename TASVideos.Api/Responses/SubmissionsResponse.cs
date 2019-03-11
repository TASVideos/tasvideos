using System.Collections.Generic;

using TASVideos.Data;
using TASVideos.Data.Entity;

#pragma warning disable 1591

namespace TASVideos.Api.Responses
{
	public class SubmissionsResponse
	{
		[Sortable]
		public int Id { get; set; }

		[Sortable]
		public string Title { get; set; }

		[Sortable]
		public string Tier { get; set; }

		[Sortable]
		public string Judge { get; set; }

		[Sortable]
		public string Publisher { get; set; }

		[Sortable]
		public string Status { get; set; }

		[Sortable]
		public string MovieExtension { get; set; }

		[Sortable]
		public int? GameId { get; set; }

		[Sortable]
		public int? RomId { get; set; }

		[Sortable]
		public string SystemCode { get; set; }

		[Sortable]
		public double? SystemFrameRate { get; set; }

		[Sortable]
		public int Frames { get; set; }
		
		[Sortable]
		public int RerecordCount { get; set; }

		[Sortable]
		public string EncodeEmbedLink { get; set; }

		[Sortable]
		public string GameVersion { get; set; }

		[Sortable]
		public string GameName { get; set; }

		[Sortable]
		public string Branch { get; set; }

		[Sortable]
		public string RomName { get; set; }
		
		[Sortable]
		public string EmulatorVersion { get; set; }

		[Sortable]
		public int? MovieStartType { get; set; }

		public IEnumerable<string> Authors { get; set; } = new string[0];
	}
}
