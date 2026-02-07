namespace TASVideos.Data.Entity;

public class WikiPageReferral
{
	public int Id { get; set; }

	public string Referrer { get; set; } = "";

	public string Referral { get; set; } = "";

	public string Excerpt { get; set; } = "";
}

public static class WikiReferralQueryableExtensions
{
	extension(IQueryable<WikiPageReferral> query)
	{
		public IQueryable<WikiPageReferral> ThatReferTo(string pageName)
			=> query.Where(wr => wr.Referral == pageName);

		public IQueryable<WikiPageReferral> ForPage(string pageName)
			=> query.Where(wr => wr.Referrer == pageName);
	}
}
