﻿namespace TASVideos.Data.Entity;

public enum UserFileClass
{
	[Display(Name = "Movie")]
	Movie,

	[Display(Name = "Support file")]
	Support
}

public enum Compression
{
	None,
	Gzip
}

[ExcludeFromHistory]
public class UserFile
{
	public long Id { get; set; }

	public int AuthorId { get; set; }
	public virtual User? Author { get; set; }

	[StringLength(255)]
	public string FileName { get; set; } = "";

	public byte[] Content { get; set; } = Array.Empty<byte>();

	public UserFileClass Class { get; set; }

	[StringLength(16)]
	public string Type { get; set; } = "";

	public DateTime UploadTimestamp { get; set; }

	public decimal Length { get; set; }

	public int Frames { get; set; }

	public int Rerecords { get; set; }

	[StringLength(255)]
	public string Title { get; set; } = "";

	public string? Description { get; set; }

	public int LogicalLength { get; set; }

	public int PhysicalLength { get; set; }

	public int? GameId { get; set; }
	public virtual Game.Game? Game { get; set; }

	public int? SystemId { get; set; }
	public virtual GameSystem? System { get; set; }

	public bool Hidden { get; set; }

	public string? Warnings { get; set; }

	public int Downloads { get; set; }

	public Compression CompressionType { get; set; }

	public string? Annotations { get; set; }

	public virtual ICollection<UserFileComment> Comments { get; set; } = new List<UserFileComment>();
}

public static class UserFileExtensions
{
	public static IQueryable<UserFile> ThatArePublic(this IQueryable<UserFile> query)
	{
		return query.Where(q => !q.Hidden);
	}

	public static IQueryable<UserFile> HideIfNotAuthor(this IQueryable<UserFile> query, int userId)
	{
		return query.Where(uf => !uf.Hidden || uf.AuthorId == userId);
	}

	public static IEnumerable<UserFile> HideIfNotAuthor(this IEnumerable<UserFile> query, int userId)
	{
		return query.Where(uf => !uf.Hidden || uf.AuthorId == userId);
	}

	public static IQueryable<UserFile> ThatAreMovies(this IQueryable<UserFile> query)
	{
		return query.Where(q => q.Class == UserFileClass.Movie);
	}

	public static IQueryable<UserFile> ThatAreSupport(this IQueryable<UserFile> query)
	{
		return query.Where(q => q.Class == UserFileClass.Support);
	}

	public static IQueryable<UserFile> ByRecentlyUploaded(this IQueryable<UserFile> query)
	{
		return query.OrderByDescending(q => q.UploadTimestamp);
	}

	public static IQueryable<UserFile> ForAuthor(this IQueryable<UserFile> query, string userName)
	{
		return query.Where(q => q.Author!.UserName == userName);
	}

	public static IQueryable<UserFile> ForGame(this IQueryable<UserFile> query, int gameId)
	{
		return query.Where(q => q.GameId == gameId);
	}
}
