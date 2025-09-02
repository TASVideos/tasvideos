using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
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
	private IDbContextTransaction? _transaction;

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

	private class TestDbContextTransaction : IDbContextTransaction
	{
		public void Dispose() { }

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;

		public void Commit() { }

		public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

		public void Rollback() { }

		public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

		public Guid TransactionId => Guid.NewGuid();
	}

	public async override Task<IDbContextTransaction> BeginTransactionAsync()
	{
		if (_transaction is null)
		{
			_transaction = await Database.BeginTransactionAsync();
			return _transaction;
		}

		return new TestDbContextTransaction(); // Send a fake one to the actual test code
	}

	public override IDbContextTransaction BeginTransaction()
	{
		if (_transaction is null)
		{
			_transaction = Database.BeginTransaction();
			return _transaction;
		}

		return new TestDbContextTransaction(); // Send a fake one to the actual test code
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

	public EntityEntry<User> AddUserWithRole(string userName, string? roleName = null)
	{
		var user = AddUser(userName);
		var name = roleName ?? "Default";
		var role = Roles.Add(new Role { Name = name, NormalizedName = name.ToUpper() }).Entity;
		UserRoles.Add(new UserRole { User = user.Entity, Role = role });
		return user;
	}

	public void AssignUserToRole(User user, Role role)
		=> UserRoles.Add(new UserRole { User = user, Role = role });

	public EntityEntry<Role> AddRoleWithPermission(PermissionTo permission)
	{
		var role = Roles.Add(new Role
		{
			Name = permission.ToString(),
			NormalizedName = permission.ToString().ToUpper()
		});

		RolePermission.Add(new RolePermission
		{
			Role = role.Entity,
			PermissionId = permission
		});

		return role;
	}

	public EntityEntry<Submission> CreatePublishableSubmission()
	{
		var entry = AddAndSaveUnpublishedSubmission();
		var submission = entry.Entity;
		submission.Status = SubmissionStatus.PublicationUnderway;
		submission.SyncedOn = DateTime.UtcNow;

		var game = Games.Add(new Game { DisplayName = "Test Game" }).Entity;
		var gameVersion = GameVersions.Add(new GameVersion { Game = game, Name = "Test Version", System = submission.System }).Entity;
		var gameGoal = GameGoals.Add(new GameGoal { Game = game, DisplayName = "Test Goal" }).Entity;
		var pubClassId = (PublicationClasses.Max(gs => (int?)gs.Id) ?? 0) + 1;
		var pubClass = PublicationClasses.Add(new PublicationClass { Id = pubClassId, Name = "Test" }).Entity;
		submission.Game = game;
		submission.GameVersion = gameVersion;
		submission.GameGoal = gameGoal;
		submission.IntendedClass = pubClass;

		SaveChanges();
		return entry;
	}

	public EntityEntry<Submission> AddAndSaveUnpublishedSubmission()
	{
		var submission = AddSubmission();
		var system = GameSystems.Add(new GameSystem { Id = 1, Code = "Default" });
		var framerate = GameSystemFrameRates.Add(new GameSystemFrameRate { GameSystemId = system.Entity.Id, FrameRate = 60.0 });
		submission.Entity.System = system.Entity;
		submission.Entity.SystemFrameRate = framerate.Entity;
		SaveChanges();

		return submission;
	}

	public EntityEntry<Submission> AddSubmission(User? submitter = null)
	{
		submitter ??= AddUser(0).Entity;
		var submission = new Submission
		{
			Submitter = submitter
		};
		return Submissions.Add(submission);
	}

	public EntityEntry<Publication> AddPublication(User? author = null, PublicationClass? publicationClass = null)
	{
		var gameSystemId = (GameSystems.Max(gs => (int?)gs.Id) ?? -1) + 1;
		var gameSystem = new GameSystem { Id = gameSystemId, Code = gameSystemId.ToString(), DisplayName = gameSystemId.ToString() };
		GameSystems.Add(gameSystem);
		var systemFrameRate = new GameSystemFrameRate { GameSystemId = gameSystem.Id };
		GameSystemFrameRates.Add(systemFrameRate);
		var game = new Game { DisplayName = "TestGame" };
		Games.Add(game);
		var gameVersion = new GameVersion { Game = game, Name = "TestGameVersion", System = gameSystem };
		GameVersions.Add(gameVersion);
		var gameGoal = new GameGoal { DisplayName = "baseline", Game = game };
		GameGoals.Add(gameGoal);
		var publicationClassId = (PublicationClasses.Max(pc => (int?)pc.Id) ?? -1) + 1;
		publicationClass ??= new PublicationClass { Id = publicationClassId, Name = publicationClassId.ToString() };
		PublicationClasses.Add(publicationClass);
		author ??= AddUser(0).Entity;
		var submission = AddSubmission(author).Entity;
		submission.Status = SubmissionStatus.Published;
		SaveChanges();

		var pub = new Publication
		{
			Title = "Test Publication",
			System = gameSystem,
			SystemFrameRate = systemFrameRate,
			Game = game,
			GameVersion = gameVersion,
			GameGoal = gameGoal,
			PublicationClass = publicationClass,
			Submission = submission,
			MovieFileName = submission.Id.ToString()
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

	public EntityEntry<ForumCategory> AddForumCategory(string? title = null)
	{
		return ForumCategories.Add(new ForumCategory { Title = title ?? "Test Category", Ordinal = 1 });
	}

	public EntityEntry<Forum> AddForum(string? name = null, bool? restricted = null)
	{
		name ??= "Test Forum";
		var category = AddForumCategory($"Category for {name}").Entity;
		return Forums.Add(new Forum { Name = name, Category = category, Restricted = restricted ?? false });
	}

	public EntityEntry<ForumTopic> AddTopic(User? createdByUser = null, bool restricted = false)
	{
		var user = createdByUser ?? AddUser(0).Entity;
		var forum = AddForum(null, restricted).Entity;
		var topic = new ForumTopic { Forum = forum, Poster = user };
		return ForumTopics.Add(topic);
	}

	public EntityEntry<ForumPost> CreatePostForTopic(ForumTopic topic, User? poster = null)
	{
		var user = poster ?? AddUser(0).Entity;
		return ForumPosts.Add(new ForumPost
		{
			Text = "Test post content",
			Topic = topic,
			Forum = topic.Forum,
			Poster = user,
			CreateTimestamp = DateTime.UtcNow
		});
	}

	public EntityEntry<ForumPoll> CreatePollForTopic(ForumTopic topic, bool isClosed = false)
	{
		var poll = ForumPolls.Add(new ForumPoll
		{
			Question = "Did you like watching this movie? ",
			MultiSelect = false,
			CloseDate = isClosed ? DateTime.UtcNow.AddDays(-5) : DateTime.UtcNow.AddDays(5)
		});

		topic.Poll = poll.Entity;

		ForumPollOptions.AddRange(
			new ForumPollOption
			{
				Poll = poll.Entity,
				Text = "Yes",
				Ordinal = 1
			},
			new ForumPollOption
			{
				Poll = poll.Entity,
				Text = "No",
				Ordinal = 2
			});

		return poll;
	}

	public EntityEntry<ForumPollOptionVote> VoteForOption(ForumPollOption option, User user)
	{
		return ForumPollOptionVotes.Add(new ForumPollOptionVote
		{
			PollOption = option,
			User = user
		});
	}

	public EntityEntry<Game> AddGame(string? displayName = null, string? abbreviation = null)
	{
		return Games.Add(new Game { DisplayName = displayName ?? "Test Game", Abbreviation = abbreviation });
	}

	public EntityEntry<Genre> AddGenre(string? displayName = null)
	{
		return Genres.Add(new Genre { DisplayName = displayName ?? "Action" });
	}

	public EntityEntry<GameGroup> AddGameGroup(string name, string? abbreviation = null)
	{
		return GameGroups.Add(new GameGroup { Name = name, Abbreviation = abbreviation });
	}

	public EntityEntry<GameGoal> AddGoalForGame(Game game, string? displayName = null)
	{
		return GameGoals.Add(new GameGoal { Game = game, DisplayName = displayName ?? "baseline" });
	}

	public EntityEntry<GameSystem> AddGameSystem(string code)
	{
		return GameSystems.Add(new GameSystem { Code = code });
	}

	public void AddForumConstantEntities()
	{
		var forumCategory = new ForumCategory();
		Forums.Add(new Forum { Id = SiteGlobalConstants.WorkbenchForumId, Category = forumCategory });
		Forums.Add(new Forum { Id = SiteGlobalConstants.PlaygroundForumId, Category = forumCategory });
		Forums.Add(new Forum { Id = SiteGlobalConstants.PublishedMoviesForumId, Category = forumCategory });
		Forums.Add(new Forum { Id = SiteGlobalConstants.GrueFoodForumId, Category = forumCategory });
		AddUser(SiteGlobalConstants.TASVideosGrueId);
		AddUser(SiteGlobalConstants.TASVideoAgentId);
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
