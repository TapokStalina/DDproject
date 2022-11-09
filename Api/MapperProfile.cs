using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.UserModel;
using AutoMapper;
using Common;

namespace Api
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, DAL.Entities.User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                ;
            CreateMap<DAL.Entities.User, UserModel>();

            CreateMap<DAL.Entities.Avatar, AttachModel>();

            CreateMap<DAL.Entities.Post, GetPostModel>()
                .ForMember(c => c.UserId, m => m.MapFrom(s => s.Author.Id));

            CreateMap<DAL.Entities.Comment, GetCommentsModel>()
                .ForMember(c => c.UserId, m => m.MapFrom(s => s.Author.Id));



        }
    }
}
