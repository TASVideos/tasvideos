using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Models
{
	public class PublicationSearchModel : IPublicationTokens
	{
		[Display(Name = "Platform")]
		public IEnumerable<string> SystemCodes { get; set; } = new List<string>();
		public IEnumerable<string> Classes { get; set; } = new List<string>();
		public IEnumerable<int> Years { get; set; } = new List<int>();
		public IEnumerable<string> Tags { get; set; } = new List<string>();
		public IEnumerable<string> Genres { get; set; } = new List<string>();
		public IEnumerable<string> Flags { get; set; } = new List<string>();

		[Display(Name = "Show Obsoleted")]
		public bool ShowObsoleted { get; set; }

		[Display(Name = "Only Obsoleted")]
		public bool OnlyObsoleted { get; set; }

		public IEnumerable<int> Authors { get; set; } = new List<int>();

		public IEnumerable<int> MovieIds { get; set; } = new List<int>();

		public IEnumerable<int> Games { get; set; } = new List<int>();

		[Display(Name = "Game Groups")]
		public IEnumerable<int> GameGroups { get; set; } = new List<int>();

		public bool IsEmpty => !SystemCodes.Any()
			&& !Classes.Any()
			&& !Years.Any()
			&& !Flags.Any()
			&& !Tags.Any()
			&& !Genres.Any()
			&& !Authors.Any()
			&& !MovieIds.Any()
			&& !Games.Any()
			&& !GameGroups.Any();

		public string ToUrl()
		{
			var sb = new StringBuilder();
			sb.Append(string.Join("-", Classes));
			if (SystemCodes.Any())
			{
				sb.Append('-').Append(string.Join("-", SystemCodes));
			}

			if (Years.Any())
			{
				sb.Append('-').Append(string.Join("-", Years.Select(y => $"Y{y}")));
			}

			if (Tags.Any())
			{
				sb.Append('-').Append(string.Join("-", Tags));
			}

			if (Flags.Any())
			{
				sb.Append('-').Append(string.Join("-", Flags));
			}

			if (Genres.Any())
			{
				sb.Append('-').Append(string.Join("-", Genres));
			}

			if (Games.Any())
			{
				sb.Append('-').Append(string.Join("-", Games.Select(g => $"{g}g")));
			}

			if (GameGroups.Any())
			{
				sb.Append('-').Append(string.Join("-", GameGroups.Select(gg => $"group{gg}")));
			}

			if (OnlyObsoleted && !IsEmpty)
			{
				sb.Append("-ObsOnly");
			}
			else if (ShowObsoleted && !IsEmpty)
			{
				sb.Append("-Obs");
			}

			return sb.ToString().Trim('-');
		}
	}
}
