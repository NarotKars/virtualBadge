using Badges.Models;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Badges.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsers users;

        public UsersController(IUsers users)
        {
            this.users = users;
        }

        [HttpPost("Login")]
        public ActionResult<Token> Login(APILogin login)
        {
            Token token = null;
            try
            {
                token = users.Login(login.Password, login.UserName, login.Email).Result;
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            if (token == null)
                return BadRequest("An error occured");
            return token;
        }

        [HttpPost("Register")]
        public ActionResult<Token> Register(APIRegister register)
        {
            Token token = null;
            try
            {
                token = users.Register(register.UserName, register.Email, register.Password, register.FirstName, register.LastName).Result;
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (token == null)
                return BadRequest("An error occured");

            return token;
        }

        [Authorize]
        [HttpGet("Get")]
        public ActionResult<User> GetUser()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var response = users.GetUser(accessToken);
            if (!string.IsNullOrWhiteSpace(response.Result.UserName))
                return response.Result;
            return BadRequest("Invalid user credentials");
        }

        [Authorize]
        [HttpGet("Search")]
        public ActionResult<List<User>> SearchUsers(string name)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            List<User> searchedUsers = null;
            try
            {
                searchedUsers = users.SearchUsers(name, handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return searchedUsers;
        }
    }
}
