using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public class WikiPageReferral
	{
		public int Id { get; set; }

		[StringLength(250)]
		public string Referrer { get; set; }

		[StringLength(250)]
		public string Referral { get; set; }

		[StringLength(250)]
		public string Excerpt { get; set; }
	}

	public static class WikiReferralQueryableExtensions
	{
		public static IQueryable<WikiPageReferral> ThatReferTo(this IQueryable<WikiPageReferral> list, string pageName)
		{
			return list.Where(wr => wr.Referrer == pageName);
		}
	}
}
