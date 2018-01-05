using AutoMapper;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos
{
	// https://stackoverflow.com/questions/40275195/how-to-setup-automapper-in-asp-net-core
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<SubmissionCreateViewModel, Submission>()
				.ForMember(dest => dest.EmulatorVersion, opt => opt.MapFrom(src => src.Emulator));

			CreateMap<User, UserEditViewModel>()
				.ForMember(dest => dest.IsLockedOut, opt => opt.MapFrom(src => src.LockoutEnabled && src.LockoutEnd.HasValue));

			CreateMap<WikiPage, UserWikiEditHistoryModel>();
		}
	}
}
