namespace TASVideos.Models
{
	public class PublicationViewModel
	{
		public int Id { get; set; }
		public int? ObsoletedBy { get; set; }
		public string Title { get; set; }
		public string Screenshot { get; set; }
		public string TorrentLink { get; set; }
		public string MovieFileName { get; set; }
		public int SubmissionId { get; set; }
		public string OnlineWatchingUrl { get; set; }
		public string MirrorSiteUrl { get; set; }
	}
}
