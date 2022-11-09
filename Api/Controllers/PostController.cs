using Api.Models.Attach;
using Api.Models.Post;
using Api.Services;
using AutoMapper;
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
        private readonly UserServices _userService;

        public PostController(UserServices userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public async Task CreatePost(List<MetadataModel> attaches, string description)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                if (attaches != null || description != null)
                {
                    PostModel model = new PostModel { Description = description, PostAttaches = attaches, Created = DateTime.UtcNow };
                    if (attaches != null)
                    {
                        List<string> paths = new List<string>();
                        foreach (var attach in attaches)
                        {
                            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), attach.TempId.ToString()));
                            if (!tempFi.Exists)
                                throw new Exception("File not found");
                            else
                            {
                                var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", attach.TempId.ToString());
                                var destFi = new FileInfo(path);
                                if (destFi.Directory != null && !destFi.Directory.Exists)
                                    destFi.Directory.Create();

                                System.IO.File.Copy(tempFi.FullName, path, true);
                                paths.Add(path);
                            }
                        }
                        await _userService.AddPost(userId, model, paths.ToArray());
                    }
                    else
                        await _userService.AddPost(userId, model);
                }
                else
                    throw new Exception("Empty post");
            }
            else
                throw new Exception("You are not authorized");
        }

        [HttpPost]
        [Authorize]
        public async Task CreateComment(Guid userId, CommentModel model)
        {
            await _userService.AddCommentToPost(userId, model);
        }

        [HttpGet]
        public async Task<List<GetCommentsModel>> GetComments(Guid postId)
        {
            return await _userService.GetCommentsFromPost(postId);
        }

        [HttpGet]
        public async Task<List<GetPostModel>> GetPosts(Guid userId)
        {
            var posts = await _userService.GetPosts(userId);
            return posts;
        }

        [HttpGet]
        public async Task<GetPostModel> GetPost(Guid userId, Guid postId)
        {
            var post = await _userService.GetPostById(userId, postId);
            return post;
        }
    }
}

