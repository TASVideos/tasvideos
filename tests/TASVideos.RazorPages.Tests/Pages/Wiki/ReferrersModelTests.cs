using TASVideos.Pages.Wiki;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class ReferrersModelTests : BasePageModelTests
{
	private readonly ReferrersModel _model;

	public ReferrersModelTests()
	{
		_model = new ReferrersModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoPath_SetsEmptyPathAndReturnsEmptyReferrals()
	{
		_model.Path = null;

		await _model.OnGet();

		Assert.AreEqual("", _model.Path);
		Assert.AreEqual(0, _model.Referrals.Count);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("/")]
	[DataRow("/ ")]
	public async Task OnGet_EmptyOrWhitespacePath_ReturnsEmptyReferrals(string path)
	{
		_model.Path = path;
		await _model.OnGet();

		Assert.AreEqual(0, _model.Referrals.Count);
	}

	[TestMethod]
	public async Task OnGet_PathWithSlashes_TrimsSlashes()
	{
		_model.Path = "/TestPage/";
		await _model.OnGet();
		Assert.AreEqual("TestPage", _model.Path);
	}

	[TestMethod]
	public async Task OnGet_PageWithNoReferrals_ReturnsEmptyList()
	{
		_model.Path = "PageWithNoReferrals";

		await _model.OnGet();

		Assert.AreEqual("PageWithNoReferrals", _model.Path);
		Assert.AreEqual(0, _model.Referrals.Count);
	}

	[TestMethod]
	public async Task OnGet_PageWithReferrals_ReturnsReferralsList()
	{
		const string targetPage = "TargetPage";
		const string referrerPage1 = "ReferrerPage1";
		const string referrerPage2 = "ReferrerPage2";

		_db.WikiReferrals.AddRange(
			new WikiPageReferral
			{
				Referrer = referrerPage1,
				Referral = targetPage,
				Excerpt = "First reference to target page"
			},
			new WikiPageReferral
			{
				Referrer = referrerPage2,
				Referral = targetPage,
				Excerpt = "Second reference to target page"
			});
		await _db.SaveChangesAsync();

		_model.Path = targetPage;

		await _model.OnGet();

		Assert.AreEqual(targetPage, _model.Path);
		Assert.AreEqual(2, _model.Referrals.Count);

		var referrers = _model.Referrals.Select(r => r.Referrer).ToList();
		Assert.IsTrue(referrers.Contains(referrerPage1));
		Assert.IsTrue(referrers.Contains(referrerPage2));

		var referral1 = _model.Referrals.First(r => r.Referrer == referrerPage1);
		var referral2 = _model.Referrals.First(r => r.Referrer == referrerPage2);

		Assert.AreEqual(targetPage, referral1.Referral);
		Assert.AreEqual(targetPage, referral2.Referral);
		Assert.AreEqual("First reference to target page", referral1.Excerpt);
		Assert.AreEqual("Second reference to target page", referral2.Excerpt);
	}

	[TestMethod]
	public async Task OnGet_OnlyReturnsReferralsToSpecificPage()
	{
		const string targetPage = "TargetPage";
		const string otherPage = "OtherPage";
		const string referrerPage = "ReferrerPage";

		_db.WikiReferrals.AddRange(
			new WikiPageReferral
			{
				Referrer = referrerPage,
				Referral = targetPage,
				Excerpt = "Reference to target page"
			},
			new WikiPageReferral
			{
				Referrer = referrerPage,
				Referral = otherPage,
				Excerpt = "Reference to other page"
			});
		await _db.SaveChangesAsync();

		_model.Path = targetPage;

		await _model.OnGet();

		Assert.AreEqual(1, _model.Referrals.Count);
		Assert.AreEqual(targetPage, _model.Referrals[0].Referral);
		Assert.AreEqual("Reference to target page", _model.Referrals[0].Excerpt);
	}

	[TestMethod]
	public async Task OnGet_MultipleReferralsFromSamePage_ReturnsAllReferrals()
	{
		const string targetPage = "TargetPage";
		const string referrerPage = "ReferrerPage";

		_db.WikiReferrals.AddRange(
			new WikiPageReferral
			{
				Referrer = referrerPage,
				Referral = targetPage,
				Excerpt = "First reference from same page"
			},
			new WikiPageReferral
			{
				Referrer = referrerPage,
				Referral = targetPage,
				Excerpt = "Second reference from same page"
			});
		await _db.SaveChangesAsync();

		_model.Path = targetPage;

		await _model.OnGet();

		Assert.AreEqual(2, _model.Referrals.Count);
		Assert.IsTrue(_model.Referrals.All(r => r.Referrer == referrerPage));
		Assert.IsTrue(_model.Referrals.All(r => r.Referral == targetPage));

		var excerpts = _model.Referrals.Select(r => r.Excerpt).ToList();
		Assert.IsTrue(excerpts.Contains("First reference from same page"));
		Assert.IsTrue(excerpts.Contains("Second reference from same page"));
	}

	[TestMethod]
	public async Task OnGet_SpecialCharactersInPath_HandlesCorrectly()
	{
		const string specialPath = "Page/With Special-Characters_123";
		const string referrerPage = "ReferrerPage";

		_db.WikiReferrals.Add(new WikiPageReferral
		{
			Referrer = referrerPage,
			Referral = specialPath,
			Excerpt = "Reference to page with special characters"
		});
		await _db.SaveChangesAsync();

		_model.Path = specialPath;

		await _model.OnGet();

		Assert.AreEqual(1, _model.Referrals.Count);
		Assert.AreEqual(specialPath, _model.Referrals[0].Referral);
	}
}
