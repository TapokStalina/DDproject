using Api.Models.Token;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserServices _userService;

        public AuthController(UserServices userServices)
        {
            _userService = userServices;
        }


        [HttpPost]
        public async Task<TokenModel> Token(TokenRequestModel model)
            => await _userService.GetToken(model.Login, model.Password);

        [HttpPost]
        public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel model)
            => await _userService.GetTokenByRefreshToken(model.RefreshToken);
    }
}
