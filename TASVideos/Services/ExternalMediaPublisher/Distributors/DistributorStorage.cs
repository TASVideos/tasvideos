using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
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

		public void Post(IPostable post)
		{
			_db.MediaPosts.Add(new MediaPosts
			{
				Title = post.Title,
				Link = post.Link,
				Body = post.Body,
				Group = post.Group,
				Type = post.Type.ToString()
			});
			_db.SaveChanges();
		}
    }
}
