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
    public class PostController : ControllerBase
    {
        private readonly PostServices _postService;
        public PostController(PostServices postService)
        {
            _postService = postService;
            _postService.SetLinkGenerator(
                linkAvatarGenerator: x =>
                Url.Action(nameof(UserController.GetUserAvatar), "User", new
                {
                    userId = x.Id,
                    download = false
                }),
                linkContentGenerator: x => Url.Action(nameof(GetPostContent), new
                {
                    postContentId = x.Id,
                    download = false
                }))
                 ;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<FileStreamResult> GetPostContent(Guid postContentId, bool download = false)
        {
            var attach = await _postService.GetPostContent(postContentId);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);

        }

        [HttpGet]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
            => await _postService.GetPosts(skip, take);

        [HttpPost]
        public async Task CreatePost(CreatePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("Not authorize");

            var model = new CreatePostModel
            {
                AuthorId = userId,
                Description = request.Description,
                Contents = request.Contents.Select(x =>
                new MetaWithPath(x, q => Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "attaches",
                    q.TempId.ToString()), userId)).ToList()
            };

            model.Contents.ForEach(x =>
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
                if (tempFi.Exists)
                {
                    var destFi = new FileInfo(x.FilePath);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    System.IO.File.Copy(tempFi.FullName, x.FilePath, true);
                    tempFi.Delete();
                }

            });

            await _postService.CreatePost(model);

        }

        [HttpPost]
        public async Task CreateComment(CreateCommentRequest request)
        {
            
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("Not authorize");

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

