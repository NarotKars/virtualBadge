using AutoMapper;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Exceptions;

namespace DataAccess
{
    public class Users : IUsers
    {
        private readonly ITokens tokens;
        private readonly IMapper mapper;
        private readonly IHttpClientFactory httpClientFactory;
        private static IConfigurationRoot configuration;
        private static HttpClient client;

        public Users(ITokens tokens, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            this.tokens = tokens;
            this.mapper = mapper;
            this.httpClientFactory = httpClientFactory;
            IConfigurationBuilder builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public async Task<Token> Login(string password, string userName = "", string email = "")
        {
            return await tokens.GetToken(password, userName, email);
        }

        public async Task<Token> Register(User user, string password)
        {
            Token token = await tokens.GetToken("admin", "admin");
            client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            string body = @$"{{
                ""username"" : ""{user.UserName}"",
                ""firstName"" : ""{user.FirstName}"",
                ""lastName"" : ""{user.LastName}"",
                ""email"" : ""{user.Email}"",
                ""enabled"" : true, {(!string.IsNullOrWhiteSpace(user.Email) ? $"\n\"emailVerified\" : true," : string.Empty)}
                ""credentials"" : [{{ ""type"": ""password"", ""value"":""{password}"", ""temporary"": false}}]
            }}";

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(configuration["Paths:RegisterUser"], content);
            Token newUserToken = null;

            if (response.IsSuccessStatusCode)
            {
                newUserToken = await tokens.GetToken(password, user.UserName);
                var handler = new JwtSecurityTokenHandler();
                string newUserId = handler.ReadJwtToken(newUserToken.access_token).Subject;
                using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                using SqlCommand command = new SqlCommand("InsertInitialBadge", connection);
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();
                command.Parameters.AddWithValue("@userId", newUserId);
                command.ExecuteNonQuery();
            }
            else
            {
                throw new KeyCloakException(response.Content.ReadAsStringAsync().Result, (int)response.StatusCode);
            }

            if (newUserToken == null || string.IsNullOrWhiteSpace(newUserToken.access_token))
                throw new KeyCloakException("Empty token", 500);

            return newUserToken;
        }

        public Task<User> GetUser(string userId)
        {
            User user = new User();
            user.Id = userId;
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("GetUserById", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@id", user.Id);
            using SqlDataReader dataReader = command.ExecuteReader();
            if (dataReader.HasRows)
            {
                if (dataReader.Read())
                {
                    user.Email = dataReader.GetString(dataReader.GetOrdinal("Email"));
                    user.FirstName = dataReader.GetString(dataReader.GetOrdinal("FirstName"));
                    user.LastName = dataReader.GetString(dataReader.GetOrdinal("LastName"));
                    user.UserName = dataReader.GetString(dataReader.GetOrdinal("UserName"));
                }
            }
            return Task.FromResult(user);
        }

        public List<User> SearchUsers(string name, string userId)
        {
            List<User> users = new List<User>();

            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("SearchUsers", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@id", userId);
            command.Parameters.AddWithValue("@str", name);
            connection.Open();
            using SqlDataReader dataReader = command.ExecuteReader();
            if (dataReader.HasRows)
            {
                int colId = dataReader.GetOrdinal("id");
                int colFirtsName = dataReader.GetOrdinal("firstName");
                int colLastName = dataReader.GetOrdinal("lastName");
                int colUserName = dataReader.GetOrdinal("userName");
                int colEmail = dataReader.GetOrdinal("email");
                int colAlreadyRequested = dataReader.GetOrdinal("alreadyRequested");
                while (dataReader.Read())
                {
                    users.Add(new User
                    {
                        Id = dataReader.GetString(colId),
                        FirstName = dataReader.GetString(colFirtsName),
                        LastName = dataReader.GetString(colLastName),
                        UserName = dataReader.GetString(colUserName),
                        Email = dataReader.GetString(colEmail),
                        AlreadyRequested = dataReader.GetInt32(colAlreadyRequested)
                    });
                }
            }
            return users;
        }
    }
}
