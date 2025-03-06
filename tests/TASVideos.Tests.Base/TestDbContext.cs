using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Tests.Base;

/// <summary>
/// Creates a context optimized for unit testing.
/// Database is in memory, and provides mechanisms for mocking database conflicts.
/// </summary>
public class TestDbContext(DbContextOptions<ApplicationDbContext> options, TestDbContext.TestHttpContextAccessor testHttpContext) : ApplicationDbContext(options, testHttpContext)
{
	private bool _dbConcurrentUpdateConflict;
	private bool _dbUpdateConflict;

	/// <summary>
	/// Simulates a user having logged in.
	/// </summary>
	public void LogInUser(string userName)
	{
		var identity = new GenericIdentity(userName);
		string[] roles = ["TestRole"];
		var principal = new GenericPrincipal(identity, roles);
		testHttpContext.HttpContext!.User = principal;
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		if (_dbUpdateConflict)
		{
			// If we need to test other types of conflicts, we can have more flags, with different exception messages
			throw new DbUpdateException("Mock update conflict scenario", new Exception("unique constraint"));
		}

		if (_dbConcurrentUpdateConflict)
		{
			throw new DbUpdateConcurrencyException("Mock concurrency conflict scenario", [new TestUpdateEntry()]);
		}

		return base.SaveChangesAsync(cancellationToken);
	}

	/// <summary>
	/// Simulates a scenario that will throw a <seealso cref="DbUpdateException"/>.
	/// These scenarios include constraint violations and model validation errors.
	/// </summary>
	public void CreateUpdateConflict()
	{
		_dbUpdateConflict = true;
	}

	/// <summary>
	/// Simulates a scenario that will throw a <seealso cref="DbUpdateConcurrencyException"/>.
	/// This happens when an optimistic concurrency check fails.
	/// </summary>
	public void CreateConcurrentUpdateConflict()
	{
		_dbConcurrentUpdateConflict = true;
	}

	public EntityEntry<User> AddUser(string userName) => AddUser(0, userName);
	public EntityEntry<User> AddUser(int userId) => AddUser(userId, "User" + userId + Guid.NewGuid());

	public EntityEntry<User> AddUser(int id, string userName)
	{
		var email = userName + "@example.com";
		var user = new User
		{
			Id = id,
			UserName = userName,
			NormalizedUserName = userName.ToUpper(),
			Email = email,
			NormalizedEmail = email.ToUpper()
		};

		return Users.Add(user);
	}

	public EntityEntry<Submission> AddSubmission(User? submitter = null)
	{
		submitter ??= AddUser(0).Entity;
		var submission = new Submission()
		{
			Submitter = submitter,
		};
		return Submissions.Add(submission);
	}

	public EntityEntry<Publication> AddPublication(User? author = null, PublicationClass? publicationClass = null)
	{
		var gameSystemId = (GameSystems.Max(gs => (int?)gs.Id) ?? -1) + 1;
		var gameSystem = new GameSystem() { Id = gameSystemId, Code = gameSystemId.ToString() };
		GameSystems.Add(gameSystem);
		var systemFrameRate = new GameSystemFrameRate() { GameSystemId = gameSystem.Id };
		GameSystemFrameRates.Add(systemFrameRate);
		var game = new Game();
		Games.Add(game);
		var gameVersion = new GameVersion() { Game = game };
		GameVersions.Add(gameVersion);
		var publicationClassId = (PublicationClasses.Max(pc => (int?)pc.Id) ?? -1) + 1;
		publicationClass ??= new PublicationClass() { Id = publicationClassId, Name = publicationClassId.ToString() };
		PublicationClasses.Add(publicationClass);
		author ??= AddUser(0).Entity;
		var submission = AddSubmission(author).Entity;
		submission.Status = SubmissionStatus.Published;
		SaveChanges();

		var pub = new Publication
		{
			Title = "Test Publication",
			SystemFrameRate = systemFrameRate,
			Game = game,
			GameVersion = gameVersion,
			PublicationClass = publicationClass,
			Submission = submission,
			MovieFileName = submission.Id.ToString(),
		};
		PublicationAuthors.Add(new PublicationAuthor { Author = author, Publication = pub });
		var pubRecord = Publications.Add(pub);
		SaveChanges();
		return pubRecord;
	}

	public EntityEntry<Publication> AddPublication(User author)
	{
		return AddPublication(author, null);
	}

	public EntityEntry<Publication> AddPublication(PublicationClass publicationClass)
	{
		return AddPublication(null, publicationClass);
	}

	public void AddForumConstantEntities()
	{
		var forumCategory = new ForumCategory();
		Forums.Add(new Forum() { Id = SiteGlobalConstants.WorkbenchForumId, Category = forumCategory });
		Forums.Add(new Forum() { Id = SiteGlobalConstants.PlaygroundForumId, Category = forumCategory });
		Forums.Add(new Forum() { Id = SiteGlobalConstants.PublishedMoviesForumId, Category = forumCategory });
		Forums.Add(new Forum() { Id = SiteGlobalConstants.GrueFoodForumId, Category = forumCategory });
		AddUser(SiteGlobalConstants.TASVideosGrueId);
		AddUser(SiteGlobalConstants.TASVideoAgentId);
	}

	public EntityEntry<ForumTopic> AddTopic()
	{
		var user = AddUser(0).Entity;
		var forumCategory = new ForumCategory();
		var forum = new Forum() { Category = forumCategory };
		var topic = new ForumTopic() { Forum = forum, Poster = user };
		return ForumTopics.Add(topic);
	}

	public class TestHttpContextAccessor : IHttpContextAccessor
	{
		public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();
	}
}

internal class TestUpdateEntry : IUpdateEntry
{
	public void SetOriginalValue(IProperty property, object? value)
	{
	}

	public void SetPropertyModified(IProperty property)
	{
	}

	public bool IsModified(IProperty property)
	{
		return false;
	}

	public bool HasTemporaryValue(IProperty property)
	{
		return false;
	}

	public bool IsStoreGenerated(IProperty property)
	{
		return false;
	}

	public object GetCurrentValue(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public object GetOriginalValue(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public TProperty GetOriginalValue<TProperty>(IProperty property)
	{
		throw new NotImplementedException();
	}

	public void SetStoreGeneratedValue(IProperty property, object? value, bool setModified = true)
	{
	}

	public EntityEntry ToEntityEntry()
	{
		return null!;
	}

	public object GetRelationshipSnapshotValue(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public object GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public bool IsConceptualNull(IProperty property)
	{
		throw new NotImplementedException();
	}

	public DbContext Context => null!;

	// ReSharper disable once UnassignedGetOnlyAutoProperty
	public IEntityType EntityType => null!;
	public EntityState EntityState { get; set; }

	// ReSharper disable once UnassignedGetOnlyAutoProperty
	public IUpdateEntry SharedIdentityEntry => null!;
}
