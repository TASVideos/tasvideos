using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki;
using TASVideos.Services;

namespace TASVideos.Test.Pages.Wiki
{
	[TestClass]
	public class RenderModelTests : BasePageModelTests
	{
		private readonly Mock<IWikiPages> _mockWikiPages;
		private readonly TestDbContext _db;
		private readonly RenderModel _model;

		public RenderModelTests()
		{
			_mockWikiPages = new Mock<IWikiPages>();
			_db = TestDbContext.Create();
			_model = new RenderModel(_mockWikiPages.Object, _db)
			{
				PageContext = TestPageContext()
			};
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow(" ")]
		public async Task Render_NullUrl_Redirects(string url)
		{
			var result = await _model.OnGet(url);
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
			var redirect = (RedirectToPageResult)result;
			Assert.AreEqual("/Wiki/PageNotFound", redirect.PageName);
		}

		[TestMethod]
		public async Task Render_ExistingPage_FindsPage()
		{
			const string existingPage = "Test";
			_mockWikiPages
				.Setup(m => m.Page(existingPage, null))
				.ReturnsAsync(new WikiPage { PageName = existingPage });

			var result = await _model.OnGet(existingPage);

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(PageResult));
		}

		[TestMethod]
		public async Task Render_HtmlExtension_StillFindsPage()
		{
			const string existingPage = "Test";
			_mockWikiPages
				.Setup(m => m.Page(existingPage, null))
				.ReturnsAsync(new WikiPage { PageName = existingPage });

			var result = await _model.OnGet(existingPage + ".html");

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(PageResult));
		}
	}
}
