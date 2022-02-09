// ReSharper disable UnusedMember.Global
namespace TASVideos.Data.Entity.Forum;

public enum ForumPostMood
{
	None = 0,
	Normal = 1,
	Angry = 2,

	[Display(Name = "Tired / Unhappy")]
	Unhappy = 3,

	[Display(Name = "Playful / Fun")]
	Playful = 4,

	[Display(Name = "Nuclear / Doom")]
	Nuclear = 5,

	[Display(Name = "Delight / Comfort")]
	Delight = 6,

	[Display(Name = "Guru / Mysterious Knowledge")]
	Guru = 7,
	Hope = 8,
	Puzzled = 9,
	Happy = 10,

	[Display(Name = "Hyperactive / Energic")]
	Hyper = 11,

	[Display(Name = "Grief / Sadness")]
	Grief = 12,
	Bleh = 13,

	[Display(Name = "Shy idea / Retreat")]
	Shy = 41,

	[Display(Name = "Plot / Scheming")]
	Plot = 42,
	Assertive = 43,

	[Display(Name = "Admin / Priority")]
	Admin = 44,

	Upset = 202,
	Xmas = 254,

	[Display(Name = "What?")]
	What = 1000,

	[Display(Name = "Alt. Normal")]
	AltNormal = 1001,

	[Display(Name = "Alt. Angry")]
	AltAngry = 1002,

	[Display(Name = "Alt. Tired / Unhappy")]
	AltUnhappy = 1003,

	[Display(Name = "Alt. Playful / Fun")]
	AltPlayful = 1004,

	[Display(Name = "Alt. Nuclear / Doom")]
	AltNuclear = 1005,

	[Display(Name = "Alt. Delight / Comfort")]
	AltDelight = 1006,

	[Display(Name = "Alt. Guru / Mysterious Knowledge")]
	AltGuru = 1007,

	[Display(Name = "Alt. Hope")]
	AltHope = 1008,

	[Display(Name = "Alt. Puzzled")]
	AltPuzzled = 1009,

	[Display(Name = "Alt. Happy")]
	AltHappy = 1010,

	[Display(Name = "Alt. Hyperactive / Energic")]
	AltHyper = 1011,

	[Display(Name = "Alt. Grief / Sadness")]
	AltGrief = 1012,

	[Display(Name = "Alt. Bleh")]
	AltBleh = 1013,

	[Display(Name = "Alt. What?")]
	AltWhat = 1201
}
