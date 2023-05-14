using TASVideos.ForumEngine;

namespace TASVideos.ForumEngineTests;
internal class TestWriterHelper : IWriterHelper
{
	public Task<string?> GetGameTitle(int id)
	{
		return Task.FromResult("TestGameTitle")!;
	}

	public Task<string?> GetGameGroupTitle(int id)
	{
		return Task.FromResult("TestGameGroupTitle")!;
	}

	public Task<string?> GetMovieTitle(int id)
	{
		return Task.FromResult("TestMovieTitle")!;
	}

	public Task<string?> GetSubmissionTitle(int id)
	{
		return Task.FromResult("TestSubmissionTitle")!;
	}
}
