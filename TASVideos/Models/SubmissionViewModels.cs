using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a movie to submit to the submission queue for the submit page
	/// </summary>
	public class SubmissionCreateViewModel
    {
		[Display(Name = "Game Version")]
		[Description("Example: USA")]
		public string GameVersion { get; set; }

		[Display(Name = "Game Name")]
		[Description("Example: Mega Man 2")]
		public string GameName { get; set; }

		[Display(Name = "Branch Name")]
		public string BranchName { get; set; }
		
		// TODO: game id

		[Display(Name = "ROM filename")]
		public string RomName { get; set; }

		[Display(Name = "Emulator and version")]
		[Description("Example: BizHawk 2.2.1")]
		public string Emulator { get; set; }

		//TODO: authors

		[Display(Name = "COmments and explanations")]
		public string Markup { get; set; }

		public byte[] MovieFile { get; set; }
    }
}
