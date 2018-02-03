namespace TASVideos.Data.Entity
{
	public class RoleLink
	{
		public int Id { get; set; }
		public string Link { get; set; }
		public virtual Role Role { get; set; }
	}
}
