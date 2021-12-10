using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IUsers
    {
        public Task<Token> Login(string password, string userName = "", string email = "");
        public Task<Token> Register(User user, string password);
        public List<User> SearchUsers(string name, string userId);
        public Task<User> GetUser(string token);
    }
}
