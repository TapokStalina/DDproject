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
using Api.Models.Like;
using Api.Migrations;

namespace Api.Services
{
    public class PostServices
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public PostServices(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePost(CreatePostRequest request)
        {
            var model = _mapper.Map<CreatePostModel>(request);

            model.Contents.ForEach(x =>
            {
                x.AuthorId = model.AuthorId;
                x.FilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "attaches",
                    x.TempId.ToString());

                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
                if (tempFi.Exists)
                {
                    var destFi = new FileInfo(x.FilePath);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    File.Move(tempFi.FullName, x.FilePath, true);
                }
            });

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
                .Include(x => x.PostContents).AsNoTracking().OrderByDescending(x => x.Created)
                .Select(x => _mapper.Map<PostModel>(x))
                .Skip(skip).Take(take).ToListAsync();
            return posts;
        }
        public async Task<List<PostModel>> GetPostsBySubscribes(Guid subscribesOwnerId, int skip, int take )
        {
            var subs = await _context.Subscribes
                .Where(x => x.SubscribeOwnerId == subscribesOwnerId)
                .AsNoTracking().ToListAsync();
            List<Guid> subscibers = new List<Guid>();
            foreach (var sub in subs)
                subscibers.Add(sub.SubscriberId);
            List<PostModel> posts = new List<PostModel>();  
            foreach (var sub in subscibers)
            { 
                var post = await _context.Posts
                    .Include(x => x.Author).Where(x => x.Id == sub)
                    .Include(x => x.PostContents).AsNoTracking().OrderByDescending(x => x.Created)
                    .Select(x => _mapper.Map<PostModel>(x))
                    .Skip(skip).Take(take).ToListAsync();
                posts.AddRange(post);
            }
            return posts;
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

        public async Task AddLikeToPost(Guid authorId, Guid postId)
        {
            var like = new LikeOnPost { AuthorId = authorId, PostId = postId };
            await _context.LikesOnPost.AddAsync(like);
            await _context.SaveChangesAsync();
        }
        public async Task AddLikeToComment(Guid authorId, Guid commentId)
        {
            var like = new LikeOnComment { AuthorId = authorId, CommentId = commentId };
            await _context.LikesOnComment.AddAsync(like);
            await _context.SaveChangesAsync();
        }
        public async Task<List<LikeModel>> GetLikesOnPost(Guid postId)
        {
            var likes = await _context.LikesOnPost.Where(x => x.PostId == postId).AsNoTracking().ToListAsync();
            var like = likes.Select(lk =>
                new LikeModel
                {
                    AuthorId = lk.AuthorId,
                    EntityId = lk.PostId,
                   
                }).ToList();
            return like;
        }
        public async Task<List<LikeModel>> GetLikesOnComment(Guid commentId)
        {
            var comments = await _context.LikesOnComment.Where(x => x.CommentId == commentId).AsNoTracking().ToListAsync();
            var comm = comments.Select(cm =>
                new LikeModel
                {
                    AuthorId = cm.AuthorId,
                    EntityId = cm.CommentId,

                }).ToList();
            return comm;
        }

    }
}

