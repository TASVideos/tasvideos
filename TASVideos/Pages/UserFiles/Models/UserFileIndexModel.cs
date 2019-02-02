using System;
using System.Collections.Generic;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles.Models
{
	public class UserFileIndexModel
	{
		public IEnumerable<UserWithMovie> UsersWithMovies { get; set; } = new List<UserWithMovie>();
		public IEnumerable<UserMovieListModel> LatestMovies { get; set; } = new List<UserMovieListModel>();
		public IEnumerable<GameWithMovie> GamesWithMovies { get; set; } = new List<GameWithMovie>();

		public class UserWithMovie
		{
			public string UserName { get; set; }
			public DateTime Latest { get; set; }
		}

		public class GameWithMovie
		{
			public int GameId { get; set; }
			public string GameName { get; set; }
			public string SystemCode { get; set; }
			public DateTime Latest { get; set; }
		}
	}
}
