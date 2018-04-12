using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TASVideos.Tasks;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;
using TASVideos.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TASVideos.Test.MVC.Tasks
{
	[TestClass]
	public class UserTaskTests
    {
		[TestMethod]
		public async Task UserTasks_GetUserNameById_ValidId_ReturnsCorrectName()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "What goes here?")
				.Options;

			using (var context = new ApplicationDbContext(options, null))
			{
				var testName = "TestUser";
				context.Users.Add(new User { Id = 1, UserName = testName });
				context.SaveChanges();
				var userTasks = new UserTasks(context, null, null); // TODO: managers

				var result = await userTasks.GetUserNameById(1);
				Assert.AreEqual(testName, result);
			}
		}
    }
}
