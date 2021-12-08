using AutoMapper;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
            Token token = null;
            try
            {
                token = await tokens.GetToken(password, userName, email);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return token;
        }

        public async Task<Token> Register(string userName, string email, string password, string firstName, string lastName)
        {
            Token token = null;
            try
            {
                token = await tokens.GetToken("admin", "admin");
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            string body = @$"{{
                ""username"" : ""{userName}"",
                ""firstName"" : ""{firstName}"",
                ""lastName"" : ""{lastName}"",
                ""email"" : ""{email}"",
                ""enabled"" : true, {(!string.IsNullOrWhiteSpace(email) ? $"\n\"emailVerified\" : true," : string.Empty)}
                ""credentials"" : [{{ ""type"": ""password"", ""value"":""{password}"", ""temporary"": false}}]
            }}";

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(configuration["Paths:RegisterUser"], content);
            Token newUserToken = null;

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    newUserToken = await tokens.GetToken(password, userName);
                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                var handler = new JwtSecurityTokenHandler();
                string newUserId = handler.ReadJwtToken(newUserToken.access_token).Subject;
                try
                {
                    using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                    using SqlCommand command = new SqlCommand("InsertInitialBadge", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    command.Parameters.AddWithValue("@userId", newUserId);
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    string errorMessage = string.Empty;
                    for (int i = 0; i < ex.Errors.Count; i++)
                    {
                        errorMessage += $"Message: {ex.Errors[i].Message}\n";
                    }
                    throw new Exception(errorMessage);
                }
            }
            else
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            return newUserToken;
        }

        public Task<User> GetUser(string token)
        {
            User user = new User();
            var handler = new JwtSecurityTokenHandler();
            user.Id = handler.ReadJwtToken(token.Split(" ")[1]).Subject;
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                using SqlCommand command = new SqlCommand("GetUserById", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                command.Parameters.AddWithValue("@id", user.Id);
                using SqlDataReader dataReader = command.ExecuteReader();
                if (dataReader.HasRows)
                {
                    if (dataReader.Read())
                    {
                        user.Id = dataReader.GetString(dataReader.GetOrdinal("Id"));
                        user.Email = dataReader.GetString(dataReader.GetOrdinal("Email"));
                        user.FirstName = dataReader.GetString(dataReader.GetOrdinal("FirstName"));
                        user.LastName = dataReader.GetString(dataReader.GetOrdinal("LastName"));
                        user.UserName = dataReader.GetString(dataReader.GetOrdinal("UserName"));
                    }
                }
            }

            catch (SqlException ex)
            {
                string errorMessages = string.Empty;
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages += $"Message: {ex.Errors[i].Message}\n";
                }
                throw new Exception(errorMessages);
            }
            return Task.FromResult(user);
        }


        public List<User> SearchUsers(string name, string userId)
        {
            List<User> users = new List<User>();
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                using SqlCommand command = new SqlCommand("SearchUsers", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
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
            }
            catch (SqlException ex)
            {
                string errorMessages = string.Empty;
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages += $"Message: {ex.Errors[i].Message}\n";
                }
                throw new Exception(errorMessages);
            }
            return users;
        }
    }
}
