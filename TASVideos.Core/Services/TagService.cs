namespace TASVideos.Core.Services;

public enum TagEditResult { Success, Fail, NotFound, DuplicateCode }
public enum TagDeleteResult { Success, Fail, NotFound, InUse }

public interface ITagService
{
	ValueTask<ICollection<Tag>> GetAll();
	ValueTask<Tag?> GetById(int id);
	ValueTask<ListDiff> GetDiff(IEnumerable<int> currentIds, IEnumerable<int> newIds);
	Task<bool> InUse(int id);
	Task<(int? Id, TagEditResult Result)> Add(string code, string displayName);
	Task<TagEditResult> Edit(int id, string code, string displayName);
	Task<TagDeleteResult> Delete(int id);
}

internal class TagService(ApplicationDbContext db, ICacheService cache) : ITagService
{
	internal const string TagsKey = "AllTags";

	public async ValueTask<ICollection<Tag>> GetAll()
	{
		if (cache.TryGetValue(TagsKey, out List<Tag> tags))
		{
			return tags;
		}

		tags = await db.Tags.ToListAsync();
		cache.Set(TagsKey, tags);
		return tags;
	}

	public async ValueTask<Tag?> GetById(int id)
	{
		var tags = await GetAll();
		return tags.SingleOrDefault(t => t.Id == id);
	}

	public async ValueTask<ListDiff> GetDiff(IEnumerable<int> currentIds, IEnumerable<int> newIds)
	{
		var tags = await GetAll();

		var currentTags = tags
			.Where(t => currentIds.Contains(t.Id))
			.Select(t => t.Code)
			.ToList();
		var newTags = tags
			.Where(t => newIds.Contains(t.Id))
			.Select(t => t.Code)
			.ToList();

		return new ListDiff(currentTags, newTags);
	}

	public async Task<bool> InUse(int id) => await db.PublicationTags.AnyAsync(pt => pt.TagId == id);

	public async Task<(int? Id, TagEditResult Result)> Add(string code, string displayName)
	{
		var entry = db.Tags.Add(new Tag
		{
			Code = code,
			DisplayName = displayName
		});

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(TagsKey);
			return (entry.Entity.Id, TagEditResult.Success);
		}
		catch (DbUpdateConcurrencyException)
		{
			return (null, TagEditResult.Fail);
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return (null, TagEditResult.DuplicateCode);
			}

			return (null, TagEditResult.Fail);
		}
	}

	public async Task<TagEditResult> Edit(int id, string code, string displayName)
	{
		var tag = await db.Tags.FindAsync(id);
		if (tag is null)
		{
			return TagEditResult.NotFound;
		}

		tag.Code = code;
		tag.DisplayName = displayName;

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(TagsKey);
			return TagEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return TagEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return TagEditResult.DuplicateCode;
			}

			return TagEditResult.Fail;
		}
	}

	public async Task<TagDeleteResult> Delete(int id)
	{
		if (await InUse(id))
		{
			return TagDeleteResult.InUse;
		}

		try
		{
			var tag = await db.Tags.FindAsync(id);
			if (tag is null)
			{
				return TagDeleteResult.NotFound;
			}

			db.Tags.Remove(tag);
			await db.SaveChangesAsync();
			cache.Remove(TagsKey);
		}
		catch (DbUpdateConcurrencyException)
		{
			return TagDeleteResult.Fail;
		}

		return TagDeleteResult.Success;
	}
}

public class UserProfile
{
	public int Id { get; init; }
	public string UserName { get; init; } = "";
	public int PlayerPoints { get; set; }
	public string PlayerRank { get; set; } = "";
	public DateTime JoinedOn { get; init; }
	public DateTime? LastLoggedIn { get; init; }
	public int PostCount { get; init; }
	public string? Avatar { get; init; }
	public string? Location { get; init; }
	public string? Signature { get; init; }
	public bool PublicRatings { get; init; }
	public string? TimeZone { get; init; }
	public PreferredPronounTypes PreferredPronouns { get; init; }

	// Private info
	public string? Email { get; init; }
	public bool EmailConfirmed { get; init; }
	public bool LockedOutStatus { get; init; }
	public DateTime? BannedUntil { get; init; }
	public string? ModeratorComments { get; init; }
	public int PublicationActiveCount { get; init; }
	public int PublicationObsoleteCount { get; init; }
	public bool HasHomePage { get; set; }
	public bool AnyPublications => PublicationActiveCount + PublicationObsoleteCount > 0;
	public IEnumerable<string> PublishedSystems { get; set; } = [];
	public WikiEdit WikiEdits { get; init; } = new();
	public PublishingSummary Publishing { get; set; } = new();
	public JudgingSummary Judgments { get; set; } = new();
	public IEnumerable<RoleSummary> Roles { get; init; } = [];
	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = [];
	public IEnumerable<SubmissionEntry> Submissions { get; set; } = [];
	public RatingSummary Ratings { get; init; } = new();
	public UserFileSummary UserFiles { get; init; } = new();
	public int SubmissionCount => Submissions.Sum(s => s.Count);
	public bool IsBanned => BannedUntil.HasValue && BannedUntil >= DateTime.UtcNow;
	public bool BanIsIndefinite => BannedUntil >= DateTime.UtcNow.AddYears(SiteGlobalConstants.YearsOfBanDisplayedAsIndefinite);

	public class SubmissionEntry
	{
		public SubmissionStatus Status { get; init; }
		public int Count { get; init; }
	}

	public class WikiEdit
	{
		public int TotalEdits { get; set; }
		public DateTime? FirstEdit { get; set; }
		public DateTime? LastEdit { get; set; }

		public DateTime FirstEditDateTime => FirstEdit ?? DateTime.UtcNow;
		public DateTime LastEditDateTime => LastEdit ?? DateTime.UtcNow;
	}

	public class RatingSummary
	{
		public int TotalMoviesRated { get; set; }
	}

	public class UserFileSummary
	{
		public int Total { get; init; }
		public IEnumerable<string> Systems { get; set; } = [];
	}

	// TODO: more data points
	public class PublishingSummary
	{
		public int TotalPublished { get; init; }
	}

	// TODO: more data points
	public class JudgingSummary
	{
		public int TotalJudgments { get; init; }
	}

	public record RoleSummary(string? Name, string Description);
}
