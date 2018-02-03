namespace TASVideos.Data.Entity
{
	public class RolePermission
	{
		public int RoleId { get; set; }
		public Role Role { get; set; }

		public PermissionTo PermissionId { get; set; }
		public bool CanAssign { get; set; }
	}
}
