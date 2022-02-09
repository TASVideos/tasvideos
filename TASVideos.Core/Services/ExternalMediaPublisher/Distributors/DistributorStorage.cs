using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

public class DistributorStorage : IPostDistributor
{
	private static readonly IEnumerable<PostType> PostTypes = Enum
		.GetValues(typeof(PostType))
		.OfType<PostType>()
		.ToList();

	private readonly ApplicationDbContext _db;

	public DistributorStorage(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<PostType> Types => PostTypes;

	public async Task Post(IPostable post)
	{
		_db.MediaPosts.Add(new MediaPost
		{
			Title = post.Title.Cap(512)!,
			Link = post.Link.Cap(255)!,
			Body = post.Body.Cap(1024)!,
			Group = post.Group.Cap(255)!,
			Type = post.Type.ToString().Cap(100)!,
			User = post.User.Cap(100)!
		});
		await _db.SaveChangesAsync();
	}
}
