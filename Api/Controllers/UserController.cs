using Api.Models;
using Api.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model) => await _userServices.CreateUser(model);


        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers() => await _userServices.GetUsers();

        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return await _userServices.GetUser(userId);
            }
            else
                throw new Exception("You are not authorized");
        }
        

    }
}
