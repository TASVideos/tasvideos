using System;

namespace TASVideos.ViewComponents.Models
{
	public class FirstEditionModel
	{
		public int Id { get; init; }
		public int GameId { get; init; }
		public string Title { get; init; } = "";
		public int TierId { get; init; }
		public string TierIconPath { get; init; } = "";
		public string TierName { get; init; } = "";
		public DateTime PublicationDate { get; init; }
	}
}
