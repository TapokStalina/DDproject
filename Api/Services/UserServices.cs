﻿using Api.Configs;
using Api.Migrations;
using Api.Models.Attach;
using Api.Models.Like;
using Api.Models.Post;
using Api.Models.Subscribe;
using Api.Models.Token;
using Api.Models.UserModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Services
{
    public class UserServices : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private readonly AuthConfig _config;
        public UserServices(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
        }
        
        public async Task<bool> CheckUserExist(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var avatar = new Avatar { Author = user, MimeType = meta.MimeType, FilePath = filePath, Name = meta.Name, Size = meta.Size };
                user.Avatar = avatar;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await GetUserById(userId);
            var atach = _mapper.Map<AttachModel>(user.Avatar);
            return atach;
        }

        public async Task Delete(Guid id)
        {
            var dbUser = await GetUserById(id);
            if (dbUser != null)
            {
                _context.Users.Remove(dbUser);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            var t = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return t.Entity.Id;
        }

        public async Task<IEnumerable<UserAvatarModel>> GetUsers() =>
             await _context.Users.AsNoTracking()
            .Include(x => x.Avatar)
            .Include(x => x.Posts)
            .Select(x => _mapper.Map<UserAvatarModel>(x))
            .ToListAsync();

        public async Task<UserAvatarModel> GetUser(Guid id) =>
            _mapper.Map<User, UserAvatarModel>(await GetUserById(id));

        private async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users
                .Include(x => x.Avatar)
                .Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                throw new Exception("User not found");
            return user;
        }

        public async Task AddSubscribe(Guid subscriberId, Guid subscribeOwnerId)
        {
            var sub = new Subscribe { SubscribeOwnerId = subscribeOwnerId, SubscriberId = subscriberId, Created = DateTime.UtcNow};
            await _context.Subscribes.AddAsync(sub);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubscribeModel>> GetAllSubscribers(Guid subscribesOwnerId)
        {
            var subscribers = await _context.Subscribes
                .Where(x => x.SubscribeOwnerId == subscribesOwnerId).AsNoTracking().ToListAsync();
            var subscribe = subscribers.Select(subs =>
                new SubscribeModel
                {
                    SubscribeOwnerId = subs.SubscribeOwnerId,
                    SubscriberId = subs.SubscriberId,

                }).ToList();
            return subscribe;
        }

        private TokenModel GenerateTokens(UserSession session)
        {
            var dtNow = DateTime.Now;
            if (session.User == null)
                throw new Exception("Magic?");

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[] {
            new Claim(ClaimsIdentity.DefaultNameClaimType, session.User.Name),
            new Claim("sessionId", session.Id.ToString()),
            new Claim("id", session.User.Id.ToString()),
            },
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[] {
                new Claim("refreshToken", session.RefreshToken.ToString()),
                },
                expires: DateTime.Now.AddHours(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);
            return new TokenModel(encodedJwt, encodedRefresh);
        }
        private async Task<User> GetUserByCredention(string login, string pass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == login.ToLower());
            if (user == null)
                throw new Exception("user not found");

            if (!HashHelper.Verify(pass, user.PasswordHash))
                throw new Exception("password is incorrect");

            return user;
        }
        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredention(login, password);
            var session = await _context.UserSessions.AddAsync(new DAL.Entities.UserSession
            {
                User = user,
                RefreshToken = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Id = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();
            return GenerateTokens(session.Entity);
        }

        public async Task<UserSession> GetSessionById(Guid id)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if (session == null)
            {
                throw new Exception("Session is not found");
            }
            return session;
        }
        private async Task<UserSession> GetSessionByRefreshToken(Guid id)
        {
            var session = await _context.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshToken == id);
            if (session == null)
            {
                throw new Exception("Session is not found");
            }
            return session;
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.SymmetricSecurityKey()
            };
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            if (principal.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value is String refreshIdString
                && Guid.TryParse(refreshIdString, out var refreshId)
                )
            {
                var session = await GetSessionByRefreshToken(refreshId);
                if (!session.IsActive)
                {
                    throw new Exception("Session is not active");
                }


                session.RefreshToken = Guid.NewGuid();
                await _context.SaveChangesAsync();

                return GenerateTokens(session);
            }
            else
            {
                throw new SecurityTokenException("Invalid token");
            }
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
