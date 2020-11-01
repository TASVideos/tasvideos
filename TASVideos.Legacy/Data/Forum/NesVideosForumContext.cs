using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Forum.Entity;

namespace TASVideos.Legacy.Data.Forum
{
	public class NesVideosForumContext : DbContext
	{
		public NesVideosForumContext(DbContextOptions<NesVideosForumContext> options) : base(options)
		{
		}

		public DbSet<Users> Users { get; set; } = null!;
		public DbSet<UserGroup> UserGroups { get; set; } = null!;
		public DbSet<UserRanks> UserRanks { get; set; } = null!;
		public DbSet<BanList> BanList { get; set; } = null!;
		public DbSet<Disallow> Disallows { get; set; } = null!;
		public DbSet<Categories> Categories { get; set; } = null!;
		public DbSet<Forums> Forums { get; set; } = null!;
		public DbSet<Topics> Topics { get; set; } = null!;
		public DbSet<Posts> Posts { get; set; } = null!;
		public DbSet<PostsText> PostsText { get; set; } = null!;
		public DbSet<TopicWatch> TopicWatch { get; set; } = null!;
		public DbSet<PrivateMessage> PrivateMessages { get; set; } = null!;
		public DbSet<PrivateMessageText> PrivateMessageText { get; set; } = null!;
		public DbSet<VoteDescription> VoteDescription { get; set; } = null!;
		public DbSet<VoteResult> VoteResult { get; set; } = null!;
		public DbSet<Voter> Voter { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Users>().ToTable("users");
			modelBuilder.Entity<BanList>().ToTable("banlist");
			modelBuilder.Entity<Categories>().ToTable("categories");
			modelBuilder.Entity<Forums>().ToTable("forums");
			modelBuilder.Entity<Topics>().ToTable("topics");
			modelBuilder.Entity<Disallow>().ToTable("disallow");

			modelBuilder.Entity<Posts>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasOne(e => e.PostText).WithOne(e => e!.Post!);
				entity.ToTable("posts");
			});
			modelBuilder.Entity<PostsText>().ToTable("posts_text");
			
			modelBuilder.Entity<PrivateMessage>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasOne(e => e.PrivateMessageText).WithOne(e => e!.PrivateMessage!);
				entity.ToTable("privmsgs");
			});
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
