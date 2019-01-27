using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Extensions
{
	/// <summary>
	/// Web front-end specific extension methods for Entity Framework POCOs
	/// </summary>
	public static class EntityExtensions
	{
		public static IQueryable<SelectListItem> ToDropdown(this IQueryable<GameSystem> query)
		{
			return query
				.Select(s => new SelectListItem
				{
					Text = s.Code,
					Value = s.Code
				});
		}

		public static IQueryable<SelectListItem> ToDropdown(this IQueryable<Tier> query)
		{
			return query
				.Select(s => new SelectListItem
				{
					Text = s.Name,
					Value = s.Id.ToString()
				});
		}
	}
}
