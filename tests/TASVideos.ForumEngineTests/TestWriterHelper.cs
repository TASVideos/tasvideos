namespace TASVideos.ForumEngineTests;

internal class TestWriterHelper : IWriterHelper
{
	private string? _gameTitle = "DefaultTestGameTitle";
	private string? _gameGroupTitle = "DefaultTestGameGroupTitle";
	private string? _submissionTitle = "DefaultTestSubmissionTitle";
	private string? _publicationTitle = "DefaultTestPublicationTitle";
	private string? _topicTitle = "DefaultTestTopicTitle";

	public Task<string?> GetGameTitle(int id)
	{
		return Task.FromResult(_gameTitle);
	}

	public Task<string?> GetGameGroupTitle(int id)
	{
		return Task.FromResult(_gameGroupTitle);
	}

	public Task<string?> GetMovieTitle(int id)
	{
		return Task.FromResult(_publicationTitle);
	}

	public Task<string?> GetSubmissionTitle(int id)
	{
		return Task.FromResult(_submissionTitle);
	}

	public Task<string?> GetTopicTitle(int id)
	{
		return Task.FromResult(_topicTitle);
	}

	internal void SetGameTitle(string title)
	{
		_gameTitle = title;
	}

	internal void SetGameGroupTitle(string? title)
	{
		_gameGroupTitle = title;
	}

	internal void SetPublicationTitle(string? title)
	{
		_publicationTitle = title;
	}

	internal void SetSubmissionTitle(string? title)
	{
		_submissionTitle = title;
	}

	internal void SetTopicTitle(string? title)
	{
		_topicTitle = title;
	}
}
