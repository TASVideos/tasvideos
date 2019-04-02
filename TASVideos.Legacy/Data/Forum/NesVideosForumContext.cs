using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Forum.Entity;

namespace TASVideos.Legacy.Data.Forum
{
	public class NesVideosForumContext : DbContext
	{
		public NesVideosForumContext(DbContextOptions<NesVideosForumContext> options) : base(options)
		{
		}

		public DbSet<Users> Users { get; set; }
		public DbSet<UserGroup> UserGroups { get; set; }
		public DbSet<UserRanks> UserRanks { get; set; }
		public DbSet<BanList> BanList { get; set; }
		public DbSet<Categories> Categories { get; set; }
		public DbSet<Forums> Forums { get; set; }
		public DbSet<Topics> Topics { get; set; }
		public DbSet<Posts> Posts { get; set; }
		public DbSet<PostsText> PostsText { get; set; }
		public DbSet<TopicWatch> TopicWatch { get; set; }
		public DbSet<PrivateMessage> PrivateMessages { get; set; }
		public DbSet<PrivateMessageText> PrivateMessageText { get; set; }
		public DbSet<VoteDescription> VoteDescription { get; set; }
		public DbSet<VoteResult> VoteResult { get; set; }
		public DbSet<Voter> Voter { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Users>().ToTable("users");
			modelBuilder.Entity<BanList>().ToTable("banlist");
			modelBuilder.Entity<Categories>().ToTable("categories");
			modelBuilder.Entity<Forums>().ToTable("forums");
			modelBuilder.Entity<Topics>().ToTable("topics");
			modelBuilder.Entity<Posts>().ToTable("posts");
			modelBuilder.Entity<PostsText>().ToTable("posts_text");
			modelBuilder.Entity<PrivateMessage>().ToTable("privmsgs");
			modelBuilder.Entity<PrivateMessageText>().ToTable("privmsgs_text");
			modelBuilder.Entity<VoteDescription>().ToTable("vote_desc");

			modelBuilder.Entity<VoteResult>(entity =>
			{
				entity.HasKey(e => new { e.Id, e.VoteOptionId });
				entity.ToTable("vote_results");
			});

			modelBuilder.Entity<Voter>(entity =>
			{
				entity.HasKey(e => new { e.Id, e.UserId, e.OptionId });
				entity.ToTable("vote_voters");
			});

			modelBuilder.Entity<UserGroup>(entity =>
			{
				entity.HasKey(e => new { e.GroupId, e.UserId });
				entity.ToTable("user_group");
			});

			modelBuilder.Entity<TopicWatch>(entity =>
			{ 
				entity.HasKey(e => new { e.TopicId, e.UserId });
				entity.ToTable("topics_watch");
			});

			modelBuilder.Entity<UserRanks>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.RankId });
				entity.ToTable("user_rank");
			});
		}
	}
}
