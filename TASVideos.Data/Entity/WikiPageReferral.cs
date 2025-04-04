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
	public static IQueryable<WikiPageReferral> ThatReferTo(this IQueryable<WikiPageReferral> query, string pageName)
		=> query.Where(wr => wr.Referral == pageName);

	public static IQueryable<WikiPageReferral> ForPage(this IQueryable<WikiPageReferral> query, string pageName)
		=> query.Where(wr => wr.Referrer == pageName);
}
