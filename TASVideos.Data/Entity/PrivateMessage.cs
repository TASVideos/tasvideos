namespace TASVideos.Data.Entity;

public class PrivateMessage : BaseEntity
{
	public int Id { get; set; }

	public int FromUserId { get; set; }
	public User? FromUser { get; set; }

	public int ToUserId { get; set; }
	public User? ToUser { get; set; }

	public string? Subject { get; set; }

	public string Text { get; set; } = "";

	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }

	public DateTime? ReadOn { get; set; } // Only a flag in the legacy system, so the date is the import date for legacy messages
	public bool SavedForFromUser { get; set; }
	public bool SavedForToUser { get; set; }

	public bool DeletedForFromUser { get; set; }
	public bool DeletedForToUser { get; set; }
}

public static class MessageExtensions
{
	public static IQueryable<PrivateMessage> SentToUser(this IQueryable<PrivateMessage> query, int userId)
		=> query.Where(m => m.ToUserId == userId);

	public static IQueryable<PrivateMessage> FromUser(this IQueryable<PrivateMessage> query, int userId)
		=> query.Where(m => m.FromUserId == userId);

	public static IQueryable<PrivateMessage> ThatAreNotToUserSaved(this IQueryable<PrivateMessage> query)
		=> query.Where(m => !m.SavedForToUser);

	public static IQueryable<PrivateMessage> ThatAreNotToUserDeleted(this IQueryable<PrivateMessage> query)
		=> query.Where(m => !m.DeletedForToUser);

	public static IQueryable<PrivateMessage> ThatAreSavedByUser(this IQueryable<PrivateMessage> query, int userId)
		=> query.Where(pm => (pm.SavedForFromUser && !pm.DeletedForFromUser && pm.FromUserId == userId)
			|| (pm.SavedForToUser && !pm.DeletedForToUser && pm.ToUserId == userId));
}
