namespace TASVideos.Data.Entity
{
	public class WikiPageReferral
	{
		public int Id { get; set; }
		public string Referrer { get; set; }
		public string Referral { get; set; }
		public string Excerpt { get; set; }
	}
}
