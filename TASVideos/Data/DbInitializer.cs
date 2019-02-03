using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Data.SampleData;
using TASVideos.Data.SeedData;
using TASVideos.Services;
using TASVideos.WikiEngine;

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
			// When the database is more mature we will move towards the Migrations process
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();
		}

		public static void Migrate(ApplicationDbContext context)
		{
			// TODO
		}

		/// <summary>
		/// Adds data necessary for production, should be run before legacy migration processes
		/// </summary>
		public static void PreMigrateSeedData(ApplicationDbContext context)
		{
			context.Roles.AddRange(RoleSeedData.AllRoles);
			context.GameSystems.AddRange(SystemSeedData.Systems);
			context.GameSystemFrameRates.AddRange(SystemSeedData.SystemFrameRates);
			context.Tiers.AddRange(TierSeedData.Tiers);
			context.Genres.AddRange(GenreSeedData.Genres);
			context.Flags.AddRange(FlagSeedData.Flags);
			context.SaveChanges();
		}

		public static void PostMigrateSeedData(ApplicationDbContext context)
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
		public static async Task GenerateDevTestUsers(ApplicationDbContext context, UserManager userManager)
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
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitrary data for testing purposes and would not be apart of a production release
		/// </summary>
		public static async Task GenerateDevSampleData(ApplicationDbContext context, UserManager userManager)
		{
			context.WikiPages.Add(PublicationSampleData.FrontPage);
			context.Games.Add(PublicationSampleData.Smb3);
			context.Roms.Add(PublicationSampleData.Smb3Rom);
			context.Submissions.Add(PublicationSampleData.MorimotoSubmission);
			context.Publications.Add(PublicationSampleData.MorimotoSmb3Pub);
			context.PublicationFlags.AddRange(PublicationSampleData.MorimotoSmb3PublicationFlags);

			await context.SaveChangesAsync();
		}
	}
}
