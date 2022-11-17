using Api.Common.Const;
using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.UserModel;
using Api.Services;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserServices _userServices;  

        public UserController(UserServices userServices)
        {
            _userServices = userServices;

            if (userServices != null)
                _userServices.SetLinkGenerator(x =>
                Url.Action(nameof(GetUserAvatar), new { userId = x.Id, download = false }));
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model)
        {
            if (await _userServices.CheckUserExist(model.Email))
                throw new Exception("User is exist");
            await _userServices.CreateUser(model);

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
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
        {
            var attach = await _userServices.GetUserAvatar(userId);

            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);
        }

        [HttpGet]
        public async Task<FileStreamResult> GetCurentUserAvatar(bool download = false)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            { 
                return await GetUserAvatar(userId, download);
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
       
    }
}
