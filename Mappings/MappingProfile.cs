using AutoMapper;
using backend.Model;

namespace backend.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<User, LoginReqDTO>();
            CreateMap<User, SignupDTO>();
            CreateMap<User, UserUpdateDTO>();
            CreateMap<Favourite, FavouriteDTO>();
            CreateMap<AllQuotes, QuoteDTO>();
            CreateMap<AllQuotes, QuoteIdDTO>();
            CreateMap<AllQuotes, AddQuoteDTO>();
            CreateMap<User, AdminUpdateUserDTO>();
        }
    }
}
