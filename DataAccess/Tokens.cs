using DataAccess.Exceptions;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataAccess
{
    public class Tokens : ITokens
    {
        private static readonly HttpClient client = new HttpClient();
        private static IConfigurationRoot configuration;

        public Tokens()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public async Task<Token> GetToken(string password, string userName = "", string email = "")
        {
            if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Either the userName or email must be provided during the login");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is not optional during login");
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var body = new Dictionary<string, string>
            {
                { "client_id", configuration["Client:Id"] },
                { "username", !string.IsNullOrWhiteSpace(userName) ? userName : email},
                { "password", password },
                { "grant_type", "password" }
            };

            var response = await client.PostAsync(configuration["Paths:Token"], new FormUrlEncodedContent(body));
            string res = await response.Content.ReadAsStringAsync();

            Token token = null;
            if (response.IsSuccessStatusCode)
            {
                token = JsonConvert.DeserializeObject<Token>(res);
            }
            else
            {
                throw new KeyCloakException(response.Content.ReadAsStringAsync().Result, (int)response.StatusCode);
            }

            if (token == null || string.IsNullOrWhiteSpace(token.access_token))
                throw new KeyCloakException("Empty token", 500);

            return token;          
        }
    }
}
