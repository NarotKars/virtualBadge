using AutoMapper;
using Badges.Models;
using DataAccess;
using DataAccess.Exceptions;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Badges.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsers users;
        private readonly IMapper mapper;

        public UsersController(IUsers users, IMapper mapper) => (this.users, this.mapper) = (users, mapper);

        [HttpPost("Login")]
        public Token Login(APILogin login)
        {
            return users.Login(login.Password, login.UserName, login.Email).Result;         
        }

        [HttpPost("Register")]
        public Token Register(APIRegister register)
        {
            return users.Register(mapper.Map<User>(register), register.Password).Result;
        }

        [Authorize]
        [HttpGet("Get")]
        public User GetUser()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            var response = users.GetUser(handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
            if (!string.IsNullOrWhiteSpace(response.Result.UserName))
                return response.Result;
            throw new KeyCloakException("Unexpected error: Empty user");
        }

        [Authorize]
        [HttpGet("Search")]
        public List<User> SearchUsers(string name)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            return users.SearchUsers(name, handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
        }
    }
}
