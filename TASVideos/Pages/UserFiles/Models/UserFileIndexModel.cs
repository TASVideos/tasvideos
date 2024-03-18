namespace TASVideos.Pages.UserFiles.Models;

public class UserFileIndexModel
{
	public IEnumerable<UserWithMovie> UsersWithMovies { get; set; } = [];
	public IEnumerable<UserMovieListModel> LatestMovies { get; set; } = [];
	public IEnumerable<GameWithMovie> GamesWithMovies { get; set; } = [];
	public IEnumerable<UncatalogedViewModel> UncatalogedFiles { get; init; } = [];

	public class UserWithMovie
	{
		public string UserName { get; set; } = "";
		public DateTime Latest { get; set; }
	}

	public class GameWithMovie
	{
		public int GameId { get; set; }
		public string GameName { get; set; } = "";
		public DateTime Latest => Dates.Max();

		internal IEnumerable<DateTime> Dates { get; set; } = [];
	}
}
