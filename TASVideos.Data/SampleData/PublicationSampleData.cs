using System;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Data.SeedData;

// ReSharper disable once CommentTypo
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
namespace TASVideos.Data.SampleData
{
	public static class PublicationSampleData
	{
		public static readonly WikiPage FrontPage = new WikiPage
		{
			PageName = "System/FrontPage",
			RevisionMessage = WikiPageSeedData.InitialCreate,
			Markup =
@"[module:ActiveTab|tab=b0]
[module:topicfeed|t=8694|right|heading=TASVideos News|l=4|hidecontent]
 
[if:!UserIsLoggedIn]
[module:DisplayMiniMovie|ratingsort=F|flags=NewcomerRec|notier]
[endif]
[if:UserIsLoggedIn]
[module:DisplayMiniMovie|ratingsort=F|tier=Stars|notier]
[endif]
%%%%

!!! Latest Publications
[module:tabularmovielist|limit=10|tier=Moons,Stars|footer=More...|flink=NewMovies]
%%%%
 
!!! [=/css/vaulttier.png|left]The [Vault]
[module:tabularmovielist|limit=10||tier=Vault|footer=More...|flink=NewVaultMovies]
%%%%

!!! Newest Submissions 
[module:FrontpageSubmissionList|maxdays=365|maxrels=5]
%%%
[Subs-List|More…]
 
!!! Newest WIPs
[module:usermovies|links=1|limit=5|dashline=1]
%%%%

!!! Contribute
Want to [Helping|help]? Everyone has something they can contribute."
		};

		public static readonly Game Smb3 = new Game
		{
			Abbreviation = "SMB3",
			DisplayName = "Super Mario Bros. 3",
			GoodName = "Super Mario Bros. 3",
			SearchKey = "nes-super-mario-bros-3",
			SystemId = 1,
			YoutubeTags = "Super Mario Bros 3,SMB3",
		};

		public static readonly GameRom Smb3Rom = new GameRom
		{
			Game = Smb3,
			Md5 = "F8DDED53BE39C303400DCFE7C5B8EC7A",
			Sha1 = "81A456C15296A31EA824A95A032CBF688E81CFDB",
			Name = "Super Mario Bros. 3 (J) (PRG0)",
			Region = "Japan",
			Version = "PRG 0",
			Type = RomTypes.Good
		};

		public static readonly byte[] MorimotoSmb3File = { 0x00 };

		public static readonly WikiPage MorimotoSmb3SubWiki = new WikiPage
		{
			PageName = "InternalSystem/SubmissionContent/S1",
			Markup = "Submission text goes here",
			RevisionMessage = ""
		};

		public static readonly WikiPage MorimotoSmb3PubWiki = new WikiPage
		{
			PageName = "InternalSystem/PublicationContent/M1",
			Markup = @"This is a historic movie, submitted and obsoleted before
the creation of the database-based (and wiki-based) site engine,
before July 2004. It was inserted into the database for a history revival project
by [user:Bisqwit] in autumn 2006.

When this video was first released on the Internet in mid-2003, its incredible quality of play became a phenomenon; since few people knew how the video was made, it was widely believed that it was played in real-time by an extremely skilled player. When [Morimoto] detailed the making of the run on his website (which can still be read [http://web.archive.org/web/20031203222907/http://soramimi.egoism.jp/emu.htm|here]), many felt deceived and turned to criticizing the video's ""illegitimacy"" instead. During this time, the concept of tool-assistance was still mostly unknown, and people even went as far as claiming that [Morimoto] had constructed the movie in several years' time by performing video editing on every single frame of the WMV.

While this video was not the first of its kind,
it was the first to gain widespread interest,
and contributed greatly to the popularity of speedruns in general.",
			RevisionMessage = ""
		};

		public static readonly Submission MorimotoSubmission = new Submission
		{
			CreateTimeStamp = DateTime.Parse("2003-11-20 01:00:00.0000000"),
			CreateUserName = "Bisqwit",
			Frames = 39837,
			Game = Smb3,
			GameVersion = "JPN",
			IntendedTierId = 2,
			MovieFile = MorimotoSmb3File,
			RerecordCount = 40268,
			RomName = "Super Mario Bros. 3 (J).nes",
			Rom = Smb3Rom,
			Status = SubmissionStatus.Published,
			SubmitterId = 1,
			SystemId = 1,
			SystemFrameRateId = 1,
			WikiContent = MorimotoSmb3SubWiki
		};

		public static readonly PublicationFile Smb3ScreenShot = new PublicationFile
		{
			Publication = MorimotoSmb3Pub,
			Path = "1M.png",
			Type = FileType.Screenshot
		};

		public static readonly PublicationFile Smb3Torrent = new PublicationFile
		{
			Publication = MorimotoSmb3Pub,
			Path = "1M.torrent",
			Type = FileType.Torrent
		};

		public static readonly Publication MorimotoSmb3Pub = new Publication
		{
			Branch = "warps",
			CreateTimeStamp = DateTime.Parse("2003-11-20 01:00:00.0000000"),
			CreateUserName = "Bisqwit",
			EmulatorVersion = "Famtasia",
			Files = new[] { Smb3ScreenShot, Smb3Torrent },
			Frames = 39837,
			Game = Smb3,
			MirrorSiteUrl = "http://www.archive.org/download/MorimotosNesSuperMarioBros.3In1103.95/smb3j-tas-morimoto.mp4",
			MovieFile = MorimotoSmb3File,
			MovieFileName = "mario3j.fmv",
			ObsoletedBy = null,
			OnlineWatchingUrl = "http://www.youtube.com/watch?v=BEcrJLM4GgU",
			PublicationFlags = MorimotoSmb3PublicationFlags,
			RerecordCount = 40268,
			Rom = Smb3Rom,
			Submission = MorimotoSubmission,
			SystemId = 1,
			SystemFrameRateId = 1,
			TierId = 1,
			Title = "TODO",
			WikiContent = MorimotoSmb3PubWiki
		};

		public static readonly PublicationFlag[] MorimotoSmb3PublicationFlags = new[]
		{
			new PublicationFlag
			{
				Flag = Array.Find(FlagSeedData.Flags, flag => flag.Token == "NewcomerRec"),
				Publication = MorimotoSmb3Pub
			}
		};
	}
}
