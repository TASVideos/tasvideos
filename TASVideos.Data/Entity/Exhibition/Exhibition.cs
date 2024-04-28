namespace TASVideos.Data.Entity.Exhibition;

[Index(nameof(PublishId), IsUnique = true)]
public class Exhibition : BaseEntity
{
	public int Id { get; set; }
	public int? PublishId { get; set; }
	public ExhibitionStatus Status { get; set; }
	public DateTime? PublicationTimestamp { get; set; }

	public string Title { get; set; } = "";
	public DateTime ExhibitionTimestamp { get; set; }

	public int? TopicId { get; set; }
	public virtual ForumTopic? Topic { get; set; }

	public virtual ICollection<Game.Game> Games { get; set; } = new HashSet<Game.Game>();
	public virtual ICollection<User> Contributors { get; set; } = new HashSet<User>();
	public virtual ICollection<ExhibitionFile> Files { get; set; } = new HashSet<ExhibitionFile>();
	public virtual ICollection<ExhibitionUrl> Urls { get; set; } = new HashSet<ExhibitionUrl>();
}

public enum ExhibitionStatus
{
	Drafted,
	Accepted,
	Published,
	Rejected,
}

public static class ExhibitionExtensions
{
	public static IQueryable<Exhibition> ThatArePublished(this IQueryable<Exhibition> exhibitions)
	{
		return exhibitions.Where(p => p.Status == ExhibitionStatus.Published);
	}

	public static IQueryable<Exhibition> ThatAreDrafts(this IQueryable<Exhibition> exhibitions)
	{
		return exhibitions.Where(p => p.Status == ExhibitionStatus.Drafted);
	}
}
