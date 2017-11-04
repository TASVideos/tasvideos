using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TASVideos.Data.Entity;
using TASVideos.Data.SampleData;
using TASVideos.Data.SeedData;

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TASVideos.Data
{
	public static class DbInitializer
	{
		/// <summary>
		/// Creates the database and seeds it with necessary seed data
		/// Seed data is necessary data for a production release
		/// </summary>
		public static void Initialize(ApplicationDbContext context)
		{
			// For now, always delete then recreate the database
			// When the datbase is more mature we will move towards the Migrations process
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			context.Permissions.AddRange(PermissionSeedData.Permissions);
			context.Roles.AddRange(RoleSeedData.Roles);
			context.SaveChanges();
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitruary data for testing purposes and would not be apart of a production release
		/// </summary>
		public static void GenerateDevSampleData(ApplicationDbContext context, UserManager<User> userManager)
		{
			foreach (var user in UserSampleData.Users)
			{
				var result = AsyncHelpers.RunSync<IdentityResult>(() => userManager
					.CreateAsync(user, UserSampleData.SamplePassword));
				if (!result.Succeeded)
				{
					throw new Exception(result.Errors.First().ToString()); // TODO
				}
			}

			var publications = new[]
			{
				new Publication { DummyProperty = "dummy1" },
				new Publication { DummyProperty = "dummy2" },
				new Publication { DummyProperty = "dummy3" },
			};

			foreach (var p in publications)
			{
				context.Publications.Add(p);
			}

			context.SaveChanges();
		}
	}

	// https://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
	// TODO: clean this up and put it somewhere useful
	public static class AsyncHelpers
	{
		/// <summary>
		/// Execute's an async Task<T> method which has a T return type synchronously
		/// </summary>
		/// <typeparam name="T">Return Type</typeparam>
		/// <param name="task">Task<T> method to execute</param>
		/// <returns></returns>
		public static T RunSync<T>(Func<Task<T>> task)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			T ret = default(T);
			synch.Post(async _ =>
			{
				try
				{
					ret = await task();
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();
			SynchronizationContext.SetSynchronizationContext(oldContext);
			return ret;
		}

		private class ExclusiveSynchronizationContext : SynchronizationContext
		{
			private bool done;
			public Exception InnerException { get; set; }
			readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
			readonly Queue<Tuple<SendOrPostCallback, object>> items =
				new Queue<Tuple<SendOrPostCallback, object>>();

			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("We cannot send to our same thread");
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				lock (items)
				{
					items.Enqueue(Tuple.Create(d, state));
				}
				workItemsWaiting.Set();
			}

			public void EndMessageLoop()
			{
				Post(_ => done = true, null);
			}

			public void BeginMessageLoop()
			{
				while (!done)
				{
					Tuple<SendOrPostCallback, object> task = null;
					lock (items)
					{
						if (items.Count > 0)
						{
							task = items.Dequeue();
						}
					}
					if (task != null)
					{
						task.Item1(task.Item2);
						if (InnerException != null) // the method threw an exeption
						{
							throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
						}
					}
					else
					{
						workItemsWaiting.WaitOne();
					}
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}
		}
	}
}
