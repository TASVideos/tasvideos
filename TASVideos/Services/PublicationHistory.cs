using TASVideos.Data;

namespace TASVideos.Services.PublicationChain
{
	public interface IPublicationHistory
	{
	}

	public class PublicationHistory : IPublicationHistory
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public PublicationHistory(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}
	}
}
