using Badges.Models;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Badges.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly ITokens tokens;

        public TokensController(ITokens tokens) => this.tokens = tokens;

        [HttpPost("RefreshToken")]
        public Token GetTokenFromRefreshToken(APIToken token)
        {
            Token t = tokens.GetTokenFromRefreshToken(token.RefreshToken).Result;
            return t;
        }
    }
}
