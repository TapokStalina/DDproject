using Api.Common.Const;
using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.Subscribe;
using Api.Models.UserModel;
using Api.Services;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class UserController : ControllerBase
    {
        private readonly UserServices _userServices;  

        public UserController(UserServices userServices, LinkGeneratorService links)
        {
            _userServices = userServices;
            links.LinkAvatarGenerator = x =>
            Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
           
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                if (!tempFi.Exists)
                    throw new Exception("File not found");
                else
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.TempId.ToString());
                    var destFi = new FileInfo(path);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    System.IO.File.Copy(tempFi.FullName, path, true);

                    await _userServices.AddAvatarToUser(userId, model, path);
                }
            }
            else
                throw new Exception("You are not authorized");
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserAvatarModel>> GetUsers() => await _userServices.GetUsers();

        [HttpGet]
        [Authorize]
        public async Task<UserAvatarModel> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return (UserAvatarModel)await _userServices.GetUser(userId);
            }
            else
                throw new Exception("You are not authorized");
        }

        [HttpPost]
        [Authorize]
        public async Task AddSubscribe(Guid subscriberId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("You are not authorized");
            await _userServices.AddSubscribe(subscriberId, userId);
        }
        [HttpPost]
        [Authorize]
        public async Task<List<SubscribeModel>> GetAllSubscribes(Guid subscribesOwnerId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("You are not authorized");
           return await _userServices.GetAllSubscribers(subscribesOwnerId);
        }
    }
}
