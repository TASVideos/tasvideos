using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Test.Data.Context
{
	[TestClass]
	public class ApplicationDbContextTests
	{
		private readonly TestDbContext _db;

		public ApplicationDbContextTests()
		{
			_db = TestDbContext.Create();
		}

		#region CreateTimeStamp

		[TestMethod]
		public void CreateTimeStamp_SetToNow_IfNotProvided()
		{
			_db.Flags.Add(new Flag());

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.IsTrue(flag.CreateTimeStamp.Year > 1);
		}

		[TestMethod]
		public void CreateTimeStamp_DoesNotOverride_IfProvided()
		{
			_db.Flags.Add(new Flag
			{
				CreateTimeStamp = DateTime.Parse("01/01/1970")
			});
			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());

			var flag = _db.Flags.Single();
			Assert.AreEqual(1970, flag.CreateTimeStamp.Year);
		}

		[TestMethod]
		public void CreateTimeStamp_OnUpdate_DoesNotChange()
		{
			var flag = new Flag
			{
				CreateTimeStamp = DateTime.Parse("01/01/1970")
			};
			_db.Flags.Add(flag);
			_db.SaveChanges();
			flag.Name = "NewName";

			_db.SaveChanges();

			Assert.AreEqual(1970, flag.CreateTimeStamp.Year);
		}

		#endregion

		#region CreateUserName
		[TestMethod]
		public void CreateUserName_SetToSystemUser_IfNotProvidedAndUserIsNull()
		{
			_db.Flags.Add(new Flag { CreateUserName = null });

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(flag.CreateUserName, ApplicationDbContext.SystemUser);
		}

		[TestMethod]
		public void CreateUserName_SetToCurrentUser_IfNotProvided()
		{
			var userName = "Batman";
			_db.LogInUser(userName);
			_db.Flags.Add(new Flag { CreateUserName = null });

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(flag.CreateUserName, userName);
		}

		[TestMethod]
		public void CreateUserName_DoesNotOverride_IfProvided()
		{
			var user = "Batman";
			_db.Flags.Add(new Flag
			{
				CreateUserName = user
			});

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(user, flag.CreateUserName);
		}

		#endregion

		#region LastUpdateTimeStamp

		[TestMethod]
		public void LastUpdateTimeStamp_OnCreate_SetToNow_IfNotProvided()
		{
			_db.Flags.Add(new Flag());

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.IsTrue(flag.LastUpdateTimeStamp.Year > 1);
		}

		[TestMethod]
		public void LastUpdateTimeStamp_OnCreate_DoesNotOverride_IfProvided()
		{
			_db.Flags.Add(new Flag
			{
				LastUpdateTimeStamp = DateTime.Parse("01/01/1970")
			});

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(1970, flag.LastUpdateTimeStamp.Year);
		}

		[TestMethod]
		public void LastUpdateTimeStamp_OnUpdate_SetToNow_IfNotProvided()
		{
			_db.Flags.Add(new Flag
			{
				LastUpdateTimeStamp = DateTime.Parse("01/01/1970")
			});

			_db.SaveChanges();
			var flag = _db.Flags.Single();
			flag.Name = "NewName";

			_db.SaveChanges();

			Assert.AreEqual(DateTime.Now.Year, flag.LastUpdateTimeStamp.Year);
		}

		[TestMethod]
		public void LastUpdateTimeStamp_OnUpdate_DoesNotOverride_IfProvided()
		{
			_db.Flags.Add(new Flag
			{
				LastUpdateTimeStamp = DateTime.Parse("01/01/1970")
			});

			_db.SaveChanges();
			var flag = _db.Flags.Single();
			flag.LastUpdateTimeStamp = DateTime.Parse("01/01/1980");

			_db.SaveChanges();

			Assert.AreEqual(1980, flag.LastUpdateTimeStamp.Year);
		}

		#endregion

		#region LastUpdateUserName

		[TestMethod]
		public void LastUpdateUserName_OnCreate_SetToSystemUser_IfNotProvidedAndUserIsNull()
		{
			_db.Flags.Add(new Flag { LastUpdateUserName = null });

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(ApplicationDbContext.SystemUser, flag.LastUpdateUserName);
		}

		[TestMethod]
		public void LastUpdateUserName_OnCreate_SetToCurrentUser_IfNotProvided()
		{
			var userName = "Batman";
			_db.Flags.Add(new Flag { LastUpdateUserName = null });
			_db.LogInUser(userName);

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(userName, flag.LastUpdateUserName);
		}

		[TestMethod]
		public void LastUpdateUserName_OnCreate_DoesNotOverride_IfProvided()
		{
			var user = "Batman";
			_db.Flags.Add(new Flag
			{
				LastUpdateUserName = user
			});

			_db.SaveChanges();

			Assert.AreEqual(1, _db.Flags.Count());
			var flag = _db.Flags.Single();
			Assert.AreEqual(user, flag.LastUpdateUserName);
		}

		[TestMethod]
		public void LastUpdateUserName_OnUpdate_DoesNotOverride_IfProvider()
		{
			var originalUser = "Batman";
			var newUser = "Joker";
			var flag = new Flag
			{
				LastUpdateUserName = originalUser
			};

			_db.Flags.Add(flag);
			_db.SaveChanges();
			flag.LastUpdateUserName = newUser;

			_db.SaveChanges();
		}

		[TestMethod]
		public void LastUpdateUserName_OnUpdate_SetToCurrentUser_IfNotProvided()
		{
			var originalUser = "Batman";
			var flag = new Flag
			{
				LastUpdateUserName = originalUser
			};
			_db.Flags.Add(flag);
			_db.SaveChanges();
			flag.Name = "NewName";

			_db.SaveChanges();

			Assert.AreEqual(1, 1);
		}

		[TestMethod]
		public void LastUpdateUserName_OnUpdate_SetToSystemUser_IfNotProvided()
		{
			var originalUser = "Batman";
			var flag = new Flag
			{
				LastUpdateUserName = originalUser
			};
			_db.Flags.Add(flag);
			_db.SaveChanges();
			flag.Name = "NewName";

			_db.SaveChanges();
		}

		#endregion
	}
}
