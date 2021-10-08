using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests
{
	internal class WikiTestCache : ICacheService
	{
		private static readonly JsonSerializerSettings SerializerSettings = new ()
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		private readonly Dictionary<string, string> _cache = new ();

		public List<WikiPage> PageCache { get; set; } = new ();

		public void Remove(string key)
		{
			var page = PageCache.SingleOrDefault(p => p.PageName == key.Split('-').Last());
			if (page != null)
			{
				PageCache.Remove(page);
			}

			_cache.Remove(key);
		}

		public void Set(string key, object? data, int? cacheTime = null)
		{
			if (data is WikiPage page)
			{
				// This is to ensure that reference equality fails
				// In a real world scenario, we would not expect the cached version
				// to be the same copy as those returned by EF queries
				PageCache.Add(new WikiPage
				{
					Id = page.Id,
					PageName = page.PageName,
					Markup = page.Markup,
					Revision = page.Revision,
					MinorEdit = page.MinorEdit,
					RevisionMessage = page.RevisionMessage,
					ChildId = page.ChildId,
					Child = page.Child,
					IsDeleted = page.IsDeleted
				});
			}

			var serialized = JsonConvert.SerializeObject(data, SerializerSettings);
			_cache[key] = serialized;
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			var result = _cache.TryGetValue(key, out string? cached);
			value = result
				? JsonConvert.DeserializeObject<T>(cached ?? "")!
				: default!;

			return result;
		}
	}
}
