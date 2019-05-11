using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using TASVideos.Data.Entity;
using TASVideos.Data.SampleData;
using TASVideos.Data.SeedData;
using TASVideos.Legacy;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;
using TASVideos.Services;
using TASVideos.WikiEngine;

namespace TASVideos.Data
{
	public static class DbInitializer
	{
		public enum StartupStrategy
		{
			/// <summary>
			/// Does nothing. Simply checks if the database exists.
			/// No data is imported, schema is not created nor validated
			/// </summary>
			Minimal,

			/// <summary>
			/// Deletes and recreates the database,
			/// Populates required seed data,
			/// Generates minimal sample data
			/// </summary>
			Sample,

			/// <summary>
			/// Deletes and recreates the database,
			/// Populates required seed data,
			/// Runs the mysql to sql server import
			/// (this options requires a connection to
			/// a mysql database of the legacy system)
			/// </summary>
			Import
		}

		public static void InitializeDatabase(IServiceProvider services)
		{
			var settings = services.GetRequiredService<IOptions<AppSettings>>();
			switch (settings.Value.StartupStrategy())
			{
				case StartupStrategy.Minimal:
					MinimalStrategy(services);
					break;
				case StartupStrategy.Sample:
					SampleStrategy(services);
					break;
				case StartupStrategy.Import:
					ImportStrategy(services);
					break;
			}
		}

		private static void MinimalStrategy(IServiceProvider services)
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			context.Database.EnsureCreated();
		}

		private static void SampleStrategy(IServiceProvider services)
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			var userManager = services.GetRequiredService<UserManager>();
			Initialize(context);

			// Note: We specifically do not want to run seed data
			// This data is already baked into the sample data file
			GenerateDevSampleData(context).Wait();
			GenerateDevTestUsers(context, userManager).Wait();
		}

		private static void ImportStrategy(IServiceProvider services)
		{
			var env = services.GetRequiredService<IHostingEnvironment>();
			var context = services.GetRequiredService<ApplicationDbContext>();
			var legacySiteContext = services.GetRequiredService<NesVideosSiteContext>();
			var legacyForumContext = services.GetRequiredService<NesVideosForumContext>();
			var userManager = services.GetRequiredService<UserManager>();
			var settings = services.GetRequiredService<IOptions<AppSettings>>().Value;

			Initialize(context);
			PreMigrateSeedData(context);
			LegacyImporter.RunLegacyImport(env, context, settings.ConnectionStrings.DefaultConnection, legacySiteContext, legacyForumContext);
			PostMigrateSeedData(context);
			GenerateDevTestUsers(context, userManager).Wait();
		}

		/// <summary>
		/// Creates the database and seeds it with necessary seed data
		/// Seed data is necessary data for a production release
		/// </summary>
		private static void Initialize(ApplicationDbContext context)
		{
			// For now, always delete then recreate the database
			// When the database is more mature we will move towards the Migrations process
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();
		}

		/// <summary>
		/// Adds data necessary for production, should be run before legacy migration processes
		/// </summary>
		private static void PreMigrateSeedData(ApplicationDbContext context)
		{
			context.Roles.AddRange(RoleSeedData.AllRoles);
			context.GameSystems.AddRange(SystemSeedData.Systems);
			context.GameSystemFrameRates.AddRange(SystemSeedData.SystemFrameRates);
			context.Tiers.AddRange(TierSeedData.Tiers);
			context.Genres.AddRange(GenreSeedData.Genres);
			context.Flags.AddRange(FlagSeedData.Flags);
			context.SubmissionRejectionReasons.AddRange(RejectionReasonsSeedData.RejectionReasons);
			context.SaveChanges();
		}

		private static void PostMigrateSeedData(ApplicationDbContext context)
		{
			foreach (var wikiPage in WikiPageSeedData.NewRevisions)
			{
				var currentRevision = context.WikiPages
					.Where(wp => wp.PageName == wikiPage.PageName)
					.SingleOrDefault(wp => wp.Child == null);

				if (currentRevision != null)
				{
					wikiPage.Revision = currentRevision.Revision + 1;
					currentRevision.Child = wikiPage;
				}

				context.WikiPages.Add(wikiPage);
				var referrals = Util.GetAllWikiLinks(wikiPage.Markup);
				foreach (var referral in referrals)
				{
					context.WikiReferrals.Add(new WikiPageReferral
					{
						Referrer = wikiPage.PageName,
						Referral = referral.Link?.Split('|').FirstOrDefault(),
						Excerpt = referral.Excerpt
					});
				}
			}

			context.SaveChanges();
		}

		/// <summary>
		/// Adds optional sample users for each role in the system for testing purposes
		/// Roles must already exist before running this
		/// DO NOT run this on production environments! This generates users with high level access and a default and public password
		/// </summary>
		private static async Task GenerateDevTestUsers(ApplicationDbContext context, UserManager userManager)
		{
			// Add users for each Role for testing purposes
			var roles = await context.Roles.ToListAsync();
			var defaultRoles = roles.Where(r => r.IsDefault).ToList();

			foreach (var role in roles.Where(r => !r.IsDefault))
			{
				// TODO: make 2 of them
				var user = new User
				{
					UserName = role.Name.Replace(" ", ""),
					NormalizedUserName = role.Name.Replace(" ", "").ToUpper(),
					Email = role.Name + "@example.com",
					TimeZoneId = "Eastern Standard Time"
				};
				var result = await userManager.CreateAsync(user, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedUser = context.Users.Single(u => u.UserName == user.UserName);
				savedUser.EmailConfirmed = true;
				savedUser.LockoutEnabled = false;
				context.UserRoles.Add(new UserRole { Role = role, User = savedUser });
				foreach (var defaultRole in defaultRoles)
				{
					context.UserRoles.Add(new UserRole { Role = defaultRole, User = savedUser });
				}
			}

			foreach (var user in UserSampleData.Users)
			{
				var result = await userManager.CreateAsync(user, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedUser = context.Users.Single(u => u.UserName == user.UserName);
				savedUser.EmailConfirmed = true;
				savedUser.LockoutEnabled = false;
				foreach (var defaultRole in defaultRoles)
				{
					context.UserRoles.Add(new UserRole { Role = defaultRole, User = savedUser });
				}
			}

			context.SaveChanges();
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitrary data for testing purposes and would not be apart of a production release
		/// </summary>
		private static async Task GenerateDevSampleData(ApplicationDbContext context)
		{
			using (await context.Database.BeginTransactionAsync())
			{
				var sql = EmbeddedSampleSqlFile();
				var commands = sql.Split("\nGO");
				foreach (var c in commands)
				{
					// EF needs this BS for some reason
					var escaped = c
						.Replace("{", "{{")
						.Replace("}", "}}");

					#pragma warning disable EF1000
					await context.Database.ExecuteSqlCommandAsync(escaped);
				}

				context.Database.CommitTransaction();
			}
		}

		private static string EmbeddedSampleSqlFile()
		{
			var stream = Assembly.GetAssembly(typeof(UserSampleData))
				.GetManifestResourceStream("TASVideos.Data.SampleData.SampleData.zip");

			var archive = new ZipArchive(stream);
			var entry = archive.Entries.Single();

			using (var tr = new StreamReader(entry.Open()))
			{
				return tr.ReadToEnd();
			}
		}
	}
}
