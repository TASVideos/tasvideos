using TASVideos.ForumEngine;

namespace TASVideos.Core.Services;

public class ForumWriterHelper : IWriterHelper
{
	private readonly ApplicationDbContext _db;

	public ForumWriterHelper(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<string?> GetMovieTitle(int id)
	{
		var publication = await _db.Publications.FirstOrDefaultAsync(p => p.Id == id);
		return publication is not null ? $"[{publication.Id}] {publication.Title}" : null;
	}

	public async Task<string?> GetSubmissionTitle(int id) => (await _db.Submissions.FirstOrDefaultAsync(s => s.Id == id))?.Title;

	public async Task<string?> GetGameTitle(int id) => (await _db.Games.FirstOrDefaultAsync(s => s.Id == id))?.DisplayName;

	public async Task<string?> GetGameGroupTitle(int id) => (await _db.GameGroups.FirstOrDefaultAsync(s => s.Id == id))?.Name;

	public async Task<string?> GetTopicTitle(int id) => (await _db.ForumTopics.FirstOrDefaultAsync(s => s.Id == id))?.Title;
}
