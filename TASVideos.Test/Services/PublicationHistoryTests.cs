using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Services;
using TASVideos.Services.PublicationChain;

namespace TASVideos.Test.Services
{
	[TestClass]
	public class PublicationHistoryTests
	{
		private IPublicationHistory _publicationHistory;
		private TestDbContext _db;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
			_publicationHistory = new PublicationHistory(_db, new NoCacheService());
		}
	}
}
