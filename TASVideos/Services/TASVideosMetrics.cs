using System.Diagnostics.Metrics;

namespace TASVideos.Services;

public interface ITASVideosMetrics
{
	public void AddUserAgent(string? userAgent);
	public void AddPublicationView(int publicationId);
	public void AddSubmissionView(int submissionId);
	public void AddWikiPageView(string wikiPageName);
	public void AddUserFileView(long userFileId);
	public void AddForumTopicView(int forumTopicId);
}

public class NullMetrics : ITASVideosMetrics
{
	public void AddUserAgent(string? userAgent) { }

	public void AddPublicationView(int publicationId) { }

	public void AddSubmissionView(int submissionId) { }

	public void AddWikiPageView(string wikiPageName) { }

	public void AddUserFileView(long userFileId) { }

	public void AddForumTopicView(int forumTopicId) { }
}

public class TASVideosMetrics : ITASVideosMetrics
{
	private readonly Counter<long> _userAgentCounter;
	private readonly Counter<long> _publicationViews;
	private readonly Counter<long> _submissionViews;
	private readonly Counter<long> _wikiPageViews;
	private readonly Counter<long> _userFileViews;
	private readonly Counter<long> _forumTopicViews;

	public TASVideosMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("TASVideos");
		_userAgentCounter = meter.CreateCounter<long>("tasvideos.useragent.count");
		_publicationViews = meter.CreateCounter<long>("tasvideos.publication.views");
		_submissionViews = meter.CreateCounter<long>("tasvideos.submission.views");
		_wikiPageViews = meter.CreateCounter<long>("tasvideos.wikipage.views");
		_userFileViews = meter.CreateCounter<long>("tasvideos.userfile.views");
		_forumTopicViews = meter.CreateCounter<long>("tasvideos.forumtopic.views");
	}

	public void AddUserAgent(string? userAgent)
	{
		_userAgentCounter.Add(1, new KeyValuePair<string, object?>("user_agent", userAgent));
	}

	public void AddPublicationView(int publicationId)
	{
		_publicationViews.Add(1, new KeyValuePair<string, object?>("publication_id", publicationId));
	}

	public void AddSubmissionView(int submissionId)
	{
		_submissionViews.Add(1, new KeyValuePair<string, object?>("submission_id", submissionId));
	}

	public void AddWikiPageView(string wikiPageName)
	{
		_wikiPageViews.Add(1, new KeyValuePair<string, object?>("wikipage_name", wikiPageName));
	}

	public void AddUserFileView(long userFileId)
	{
		_userFileViews.Add(1, new KeyValuePair<string, object?>("userfile_id", userFileId));
	}

	public void AddForumTopicView(int forumTopicId)
	{
		_forumTopicViews.Add(1, new KeyValuePair<string, object?>("forumtopic_id", forumTopicId));
	}
}
