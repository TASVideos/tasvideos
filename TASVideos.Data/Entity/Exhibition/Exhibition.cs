using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASVideos.Data.Entity.Exhibition;
public class Exhibition : BaseEntity
{
	public int Id { get; set; }
	//public int? PublicId { get; set; }
	public virtual ICollection<Game.Game> Games { get; set; } = new HashSet<Game.Game>();
	public virtual ICollection<User> Contributors { get; set; } = new HashSet<User>();
	public virtual ICollection<ExhibitionFile> Files { get; set; } = new HashSet<ExhibitionFile>();
	public virtual ICollection<ExhibitionUrl> Urls { get; set; } = new HashSet<ExhibitionUrl>();

	public string Title { get; set; } = "";

	public DateTime ExhibitionTimestamp { get; set; }
}
