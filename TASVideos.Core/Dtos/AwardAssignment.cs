using TASVideos.Data.Entity.Awards;

namespace TASVideos.Core.Services;

public interface IAwardAssignmentSummary
{
	string ShortName { get; }
	string Description { get; }
	int Year { get; }
}

/// <summary>
/// Represents the assignment of an award to a user or movie
/// Ex: 2010 TASer of the Year.
/// </summary>
public class AwardAssignment : IAwardAssignmentSummary
{
	public int AwardId { get; init; }
	public string ShortName { get; init; } = "";
	public string Description { get; init; } = "";
	public int Year { get; init; }
	public AwardType Type { get; init; }
	public IReadOnlyCollection<Publication> Publications { get; init; } = [];
	public IReadOnlyCollection<User> Users { get; init; } = [];

	public record User(int Id, string UserName);
	public record Publication(int Id, string Title);
}

/// <summary>
/// Represents a short summary of an award assignment
/// </summary>
public record AwardAssignmentSummary(string ShortName, string Description, int Year) : IAwardAssignmentSummary;
