using Api.Models.Token;
using Api.Models.UserModel;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserServices _userServices;

        public AuthController(UserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost]
        public async Task<TokenModel> Token(TokenRequestModel model)
            => await _userServices.GetToken(model.Login, model.Password);

        [HttpPost]
        public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel model)
            => await _userServices.GetTokenByRefreshToken(model.RefreshToken);

        [HttpPost]
        public async Task CreateUser(CreateUserModel model)
        {
            if (await _userServices.CheckUserExist(model.Email))
                throw new Exception("User is exist");
            await _userServices.CreateUser(model);
        }
    }
}
