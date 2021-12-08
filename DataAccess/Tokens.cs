using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            return token;          
        }

        public async Task<Token> GetServiceToken()
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var body = new Dictionary<string, string>
            {
                { "client_id", configuration["Client:Id"] },
                { "client_secret", configuration["Client:Secret"] },
                {"grant_type", "client_credentials" }
            };

            var response = await client.PostAsync(configuration["Paths:Token"], new FormUrlEncodedContent(body));
            string res = await response.Content.ReadAsStringAsync();

            Token token = null;
            if (response.IsSuccessStatusCode)
            {
                token = JsonConvert.DeserializeObject<Token>(res);
            }
            return token;
        }
    }
}
