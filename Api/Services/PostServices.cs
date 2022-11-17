using Api.Configs;
using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.UserModel;
using AutoMapper;
using DAL.Entities;
using DAL;
using Common.Extentions;
using Api.Common.Const;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;


namespace Api.Services
{
    public class PostServices
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private Func<AttachModel, string?>? _linkContentGenerator;
        private Func<UserModel, string?>? _linkAvatarGenerator;
        public void SetLinkGenerator(Func<AttachModel, string?> linkContentGenerator, Func<UserModel, string?> linkAvatarGenerator)
        {
            _linkAvatarGenerator = linkAvatarGenerator;
            _linkContentGenerator = linkContentGenerator;
        }
        public PostServices(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePost(CreatePostModel model)
        {
            var dbModel = _mapper.Map<Post>(model);

            await _context.Posts.AddAsync(dbModel);
            await _context.SaveChangesAsync();
        }
        public async Task CreateComment(CreateCommentModel model)
        {
            var dbModel = _mapper.Map<Comment>(model);

            await _context.Comments.AddAsync(dbModel);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostContents).AsNoTracking().Take(take).Skip(skip).ToListAsync();

            var res = posts.Select(post =>
                new PostModel
                {
                    Author = new UserAvatarModel(_mapper.Map<UserModel>(post.Author), post.Author.Avatar == null ? null : _linkAvatarGenerator),
                    Description = post.Description,
                    Id = post.Id,
                    Contents = post.PostContents?.Select(x =>
                    new AttachWithLinkModel(_mapper.Map<AttachModel>(x), _linkContentGenerator)).ToList()
                }).ToList();

            return res;
        }

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var res = await _context.PostContents.FirstOrDefaultAsync(x => x.Id == postContentId);
            return _mapper.Map<AttachModel>(res);
        }

        public async Task<List<CommentModel>> GetComments(Guid postId)
        { 
            var comments = await _context.Comments.Where(x => x.PostId == postId).AsNoTracking().ToListAsync();
            
            var comms = comments.Select(comment =>
                new CommentModel
                {
                    AuthorId = comment.AuthorId,
                    CommentText = comment.CommentText,
                    CommentId = comment.CommentId,
                    PostId = postId,

               }).ToList();
                
            return comms;
        }
    }
}

