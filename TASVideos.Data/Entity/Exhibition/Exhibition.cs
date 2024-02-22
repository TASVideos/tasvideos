namespace TASVideos.Data.Entity.Exhibition;
public class Exhibition : BaseEntity
{
	public int Id { get; set; }
	public int? PublishId { get; set; }
	public bool IsPublished { get; set; }
	public DateTime PublicationTimestamp { get; set; }

	public string Title { get; set; } = "";
	public DateTime ExhibitionTimestamp { get; set; }

	public virtual ICollection<Game.Game> Games { get; set; } = new HashSet<Game.Game>();
	public virtual ICollection<User> Contributors { get; set; } = new HashSet<User>();
	public virtual ICollection<ExhibitionFile> Files { get; set; } = new HashSet<ExhibitionFile>();
	public virtual ICollection<ExhibitionUrl> Urls { get; set; } = new HashSet<ExhibitionUrl>();
}

public static class ExhibitionExtensions
{
	public static IQueryable<Exhibition> ThatArePublished(this IQueryable<Exhibition> exhibitions)
	{
		return exhibitions.Where(p => p.IsPublished);
	}

	public static IQueryable<Exhibition> ThatAreDrafts(this IQueryable<Exhibition> exhibitions)
	{
		return exhibitions.Where(p => !p.IsPublished);
	}
}
