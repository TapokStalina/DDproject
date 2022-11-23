using Api.Common.Const;
using Api.Models.Attach;
using Api.Models.Like;
using Api.Models.Post;
using Api.Services;
using AutoMapper;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class PostController : ControllerBase
    {
        private readonly PostServices _postService;
        public PostController(PostServices postService, LinkGeneratorService links)
        {
            _postService = postService;
            links.LinkContentGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostContent), new
            {
                postContentId = x.Id,
            });
            links.LinkAvatarGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }

        [HttpGet]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
            => await _postService.GetPosts(skip, take);
        [HttpGet]
        public async Task<List<PostModel>> GetPostsBySubscribes( Guid subscribesOwnerId, int skip = 0, int take = 10)
            => await _postService.GetPostsBySubscribes(subscribesOwnerId, skip, take );

        [HttpPost]
        public async Task CreatePost(CreatePostRequest request)
        {
                var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
                if (userId == default)
                    throw new Exception("You are not authorized");
                request.AuthorId = userId;
         
            await _postService.CreatePost(request);
        }

        [HttpPost]
        public async Task CreateComment(CreateCommentRequest request)
        {
            
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("You are not authorized");

            var model = new CreateCommentModel
            {
                CommentText = request.CommentText,
                CommentId = new Guid(),
                PostId = request.PostId,
                AuthorId = userId,
                Created = DateTime.Now,

            };
            await _postService.CreateComment(model);
        }
        [HttpGet]
        public async Task<List<CommentModel>> GetComments(Guid postId)
             => await _postService.GetComments(postId);

        [HttpPost]
        [Authorize]
        public async Task AddLikeToPost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("You are not authorized");
            await _postService.AddLikeToPost(userId, postId);
        }
        [HttpPost]
        [Authorize]
        public async Task AddLikeToComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("You are not authorized");
            await _postService.AddLikeToComment(userId, commentId);
        }

        [HttpGet]
        public async Task<List<LikeModel>> GetLikesOnPost(Guid postId)
        {
            return await _postService.GetLikesOnPost(postId);
        }
        [HttpGet]
        public async Task<List<LikeModel>> GetLikesOnComment(Guid commentId)
        {
            return await _postService.GetLikesOnComment(commentId);
        }
    }
}

