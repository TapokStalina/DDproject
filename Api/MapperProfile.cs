using Api.Models.Attach;
using Api.Models.Like;
using Api.Models.Post;
using Api.Models.UserModel;
using AutoMapper;
using Common;
using DAL.Entities;

namespace Api
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                ;
            CreateMap<User, UserModel>();
            CreateMap<User, UserAvatarModel>();

            CreateMap<Avatar, AttachModel>();
            CreateMap<PostContent, AttachModel >();
            CreateMap<PostContent, AttachExternalModel>();


            CreateMap<CreatePostRequest, CreatePostModel>();
            CreateMap<MetadataModel, MetadataLinkModel>();
            CreateMap<MetadataLinkModel, PostContent>();

            CreateMap<CreatePostModel, Post>()
                .ForMember(d => d.PostContents, m => m.MapFrom(s => s.Contents))
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<CreateCommentModel, Comment>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTime.UtcNow));
            CreateMap<CreateCommentModel, Post>()
                .ForMember(d => d.Id, m => m.MapFrom(s => s.PostId))
                .ForMember(d => d.CommentContents, m => m.MapFrom(s => s.Contents));

            CreateMap<LikeOnPost, LikeModel>()
               .ForMember(d => d.EntityId, m => m.MapFrom(s => s.PostId));
            CreateMap<LikeOnComment, LikeModel>()
              .ForMember(d => d.EntityId, m => m.MapFrom(s => s.CommentId));




        }
    }
}
