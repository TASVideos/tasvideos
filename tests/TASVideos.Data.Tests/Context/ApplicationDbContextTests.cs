using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data.Entity;
using TASVideos.Tests.Base;

namespace TASVideos.Data.Tests.Context
{
	[TestClass]
	public class ApplicationDbContextTests
	{
		private readonly TestDbContext _db;

		public ApplicationDbContextTests()
		{
			_db = TestDbContext.Create();
		}

		#region CreateTimestamp

		[TestMethod]
		public async Task CreateTimestamp_SetToNow_IfNotProvided()
		{
			_db.Flags.Add(new Flag());

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.IsTrue(flag.CreateTimestamp.Year > 1);
		}

		[TestMethod]
		public async Task CreateTimestamp_DoesNotOverride_IfProvided()
		{
			_db.Flags.Add(new Flag
			{
				CreateTimestamp = DateTime.Parse("01/01/1970")
			});
			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());

			var flag = _db.Flags.Single();
			Assert.AreEqual(1970, flag.CreateTimestamp.Year);
		}

		[TestMethod]
		public async Task CreateTimestamp_OnUpdate_DoesNotChange()
		{
			var flag = new Flag
			{
				CreateTimestamp = DateTime.Parse("01/01/1970")
			};
			_db.Flags.Add(flag);
			await _db.SaveChangesAsync();
			flag.Name = "NewName";

			await _db.SaveChangesAsync();

			Assert.AreEqual(1970, flag.CreateTimestamp.Year);
		}

		#endregion

		#region CreateUserName
		[TestMethod]
		public async Task CreateUserName_SetToSystemUser_IfNotProvidedAndUserIsNull()
		{
			_db.Flags.Add(new Flag { CreateUserName = null });

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(flag.CreateUserName, ApplicationDbContext.SystemUser);
		}

		[TestMethod]
		public async Task CreateUserName_SetToCurrentUser_IfNotProvided()
		{
			var userName = "Batman";
			_db.LogInUser(userName);
			_db.Flags.Add(new Flag { CreateUserName = null });

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(flag.CreateUserName, userName);
		}

		[TestMethod]
		public async Task CreateUserName_DoesNotOverride_IfProvided()
		{
			var user = "Batman";
			_db.Flags.Add(new Flag
			{
				CreateUserName = user
			});

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(user, flag.CreateUserName);
		}

		#endregion

		#region LastUpdateTimestamp

		[TestMethod]
		public async Task LastUpdateTimestamp_OnCreate_SetToNow_IfNotProvided()
		{
			_db.Flags.Add(new Flag());

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.IsTrue(flag.LastUpdateTimestamp.Year > 1);
		}

		[TestMethod]
		public async Task LastUpdateTimestamp_OnCreate_DoesNotOverride_IfProvided()
		{
			_db.Flags.Add(new Flag
			{
				LastUpdateTimestamp = DateTime.Parse("01/01/1970")
			});

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(1970, flag.LastUpdateTimestamp.Year);
		}

		[TestMethod]
		public async Task LastUpdateTimestamp_OnUpdate_SetToNow_IfNotProvided()
		{
			_db.Flags.Add(new Flag
			{
				LastUpdateTimestamp = DateTime.Parse("01/01/1970")
			});

			await _db.SaveChangesAsync();
			var flag = _db.Flags.Single();
			flag.Name = "NewName";

			await _db.SaveChangesAsync();

			Assert.AreEqual(DateTime.UtcNow.Year, flag.LastUpdateTimestamp.Year);
		}

		[TestMethod]
		public async Task LastUpdateTimestamp_OnUpdate_DoesNotOverride_IfProvided()
		{
			_db.Flags.Add(new Flag
			{
				LastUpdateTimestamp = DateTime.Parse("01/01/1970")
			});

			await _db.SaveChangesAsync();
			var flag = _db.Flags.Single();
			flag.LastUpdateTimestamp = DateTime.Parse("01/01/1980");

			await _db.SaveChangesAsync();

			Assert.AreEqual(1980, flag.LastUpdateTimestamp.Year);
		}

		#endregion

		#region LastUpdateUserName

		[TestMethod]
		public async Task LastUpdateUserName_OnCreate_SetToSystemUser_IfNotProvidedAndUserIsNull()
		{
			_db.Flags.Add(new Flag { LastUpdateUserName = null });

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(ApplicationDbContext.SystemUser, flag.LastUpdateUserName);
		}

		[TestMethod]
		public async Task LastUpdateUserName_OnCreate_SetToCurrentUser_IfNotProvided()
		{
			var userName = "Batman";
			_db.Flags.Add(new Flag { LastUpdateUserName = null });
			_db.LogInUser(userName);

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(userName, flag.LastUpdateUserName);
		}

		[TestMethod]
		public async Task LastUpdateUserName_OnCreate_DoesNotOverride_IfProvided()
		{
			var user = "Batman";
			_db.Flags.Add(new Flag
			{
				LastUpdateUserName = user
			});

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(user, flag.LastUpdateUserName);
		}

		[TestMethod]
		public async Task LastUpdateUserName_OnUpdate_DoesNotOverride_IfProvider()
		{
			var originalUser = "Batman";
			var newUser = "Joker";
			var flag = new Flag
			{
				LastUpdateUserName = originalUser
			};

			_db.Flags.Add(flag);
			await _db.SaveChangesAsync();
			flag.LastUpdateUserName = newUser;

			await _db.SaveChangesAsync();
		}

		[TestMethod]
		public async Task LastUpdateUserName_OnUpdate_SetToCurrentUser_IfNotProvided()
		{
			var originalUser = "Batman";
			var flag = new Flag
			{
				LastUpdateUserName = originalUser
			};
			_db.Flags.Add(flag);
			await _db.SaveChangesAsync();
			flag.Name = "NewName";

			await _db.SaveChangesAsync();

			Assert.AreEqual(1, 1);
		}

		[TestMethod]
		public async Task LastUpdateUserName_OnUpdate_SetToSystemUser_IfNotProvided()
		{
			var originalUser = "Batman";
			var flag = new Flag
			{
				LastUpdateUserName = originalUser
			};
			_db.Flags.Add(flag);
			await _db.SaveChangesAsync();
			flag.Name = "NewName";

			await _db.SaveChangesAsync();
		}

		#endregion
	}
}
