using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a movie to submit to the submission queue for the submit page
	/// </summary>
	public class SubmissionCreateViewModel
    {
		public string GameVersion { get; set; }
		public string GameName { get; set; }
		public string BranchName { get; set; }
		// TODO: game id
		public string RomName { get; set; }
		public string Emulator { get; set; }
		//TODO: authors
		public string Markup { get; set; }
		public byte[] MovieFile { get; set; }
    }
}
