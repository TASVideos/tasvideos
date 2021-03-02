using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data.Entity.Forum;
using TASVideos.Services;

namespace TASVideos.Test.Services
{
	[TestClass]
	public class TASVideosGrueTests
	{
		private const int SubmissionId = 1;

		private readonly ITASVideosGrue _tasVideosGrue;
		private readonly TestDbContext _db;

		public TASVideosGrueTests()
		{
			_db = TestDbContext.Create();
			_tasVideosGrue = new TASVideosGrue(_db);
		}

		[TestMethod]
		public async Task PostSubmissionRejection_NoTopic_DoesNotPost()
		{
			await _tasVideosGrue.PostSubmissionRejection(SubmissionId);
			var actual = await _db.ForumPosts.LastOrDefaultAsync();
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task PostSubmissionRejection_TopicCreated()
		{
			_db.ForumTopics.Add(new ForumTopic
			{
				Title = "Title",
				PageName = LinkConstants.SubmissionWikiPage + SubmissionId
			});
			await _db.SaveChangesAsync();

			await _tasVideosGrue.PostSubmissionRejection(SubmissionId);
			var actual = await _db.ForumPosts.LastOrDefaultAsync();

			Assert.IsNotNull(actual);
			Assert.AreEqual(SiteGlobalConstants.TASVideosGrue, actual.CreateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideosGrue, actual.LastUpdateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideosGrueId, actual.PosterId);
			Assert.IsFalse(actual.EnableHtml);
			Assert.IsFalse(actual.EnableBbCode);
			Assert.IsNotNull(actual.Text);
			Assert.IsFalse(actual.Text.Contains("stale"));
		}

		[TestMethod]
		public async Task PostSubmissionRejection_StaleIfOld()
		{
			_db.ForumTopics.Add(new ForumTopic
			{
				CreateTimeStamp = DateTime.Now.AddYears(-1),
				Title = "Title",
				PageName = LinkConstants.SubmissionWikiPage + SubmissionId
			});
			await _db.SaveChangesAsync();

			await _tasVideosGrue.PostSubmissionRejection(SubmissionId);
			var actual = await _db.ForumPosts.LastOrDefaultAsync();

			Assert.IsNotNull(actual);
			Assert.AreEqual(SiteGlobalConstants.TASVideosGrue, actual.CreateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideosGrue, actual.LastUpdateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideosGrueId, actual.PosterId);
			Assert.IsFalse(actual.EnableHtml);
			Assert.IsFalse(actual.EnableBbCode);
			Assert.IsNotNull(actual.Text);
			Assert.IsTrue(actual.Text.Contains("stale"));
		}
	}
}
