using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data;
using TASVideos.Services;

namespace TASVideos.Test.Services
{
	[TestClass]
	public class TASVideoAgentTests
	{
		private const int SubmissionId = 1;
		private const string SubmissionTitle = "Test Title";

		private ITASVideoAgent _tasVideoAgent = null!;
		private TestDbContext _db = null!;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
			_tasVideoAgent = new TASVideoAgent(_db);
		}

		[TestMethod]
		public async Task PostSubmissionTopic_CreatesPost()
		{
			await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
			var actual = await _db.ForumPosts.LastOrDefaultAsync();

			Assert.IsNotNull(actual);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
			Assert.IsTrue(actual.EnableHtml);
			Assert.IsFalse(actual.EnableBbCode);
		}

		[TestMethod]
		public async Task PostSubmissionTopic_CreatesTopic()
		{
			await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
			var actual = await _db.ForumTopics.LastOrDefaultAsync();

			Assert.IsNotNull(actual);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
			Assert.AreEqual(LinkConstants.SubmissionWikiPage + SubmissionId, actual.PageName);
			Assert.AreEqual(SubmissionTitle, actual.Title);
			Assert.AreEqual(ForumConstants.WorkBenchForumId, actual.ForumId);
		}

		[TestMethod]
		public async Task PostSubmissionTopic_CreatesPoll()
		{
			await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
			var actual = await _db.ForumPolls.LastOrDefaultAsync();

			Assert.IsNotNull(actual);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
			Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
			Assert.AreEqual(SiteGlobalConstants.PollQuestion, actual.Question);
		}

		[TestMethod]
		public async Task PostSubmissionTopic_CreatesPollOptions()
		{
			await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
			var actual = await _db.ForumPollOptions.ToListAsync();

			Assert.AreEqual(3, actual.Count);
			Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionNo));
			Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionYes));
			Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionsMeh));
		}

		[TestMethod]
		public async Task PostSubmissionTopic_IdsMatch()
		{
			await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);

			var post = await _db.ForumPosts.LastOrDefaultAsync();
			var topic = await _db.ForumTopics.LastOrDefaultAsync();
			var poll = await _db.ForumPolls.LastOrDefaultAsync();
			var options = await _db.ForumPollOptions.ToListAsync();

			Assert.IsNotNull(post);
			Assert.IsNotNull(topic);
			Assert.IsNotNull(poll);

			Assert.AreEqual(topic.Id, post.TopicId);
			Assert.AreEqual(topic.Id, poll.TopicId);
			Assert.IsTrue(options.All(o => o.PollId == poll.Id));
		}
	}
}
