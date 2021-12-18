using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface ITokens
    {
        public Task<Token> GetToken(string password, string userName = "", string email = "");
        public Task<Token> GetTokenFromRefreshToken(string accessToken);
    }
}
