using AutoMapper;
using Badges.Models;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Badges.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequests requests;
        private readonly IMapper mapper;

        public RequestsController(IRequests requests, IMapper mapper) => (this.requests, this.mapper) = (requests, mapper);

        [Authorize]
        [HttpPost("Create")]
        public int CreateRequest(APIRequest request)
        {
            int newRequestId = 0;
            newRequestId = requests.CreateRequest(mapper.Map<Request>(request));

            if (newRequestId == 0)
                throw new Exception("Unknown exception while creating badge request");

            return newRequestId;
        }

        [Authorize]
        [HttpPut("Accept")]
        public ActionResult AcceptRequest(int id)
        {
            requests.AcceptRequest(id);
            return Ok("The request is successfully accepted");
        }

        [Authorize]
        [HttpPut("Decline")]
        public ActionResult DeclineRequest(int id)
        {
            requests.DeclineRequest(id);
            return Ok("The request is successfully declined");
        }

        [Authorize]
        [HttpGet("Received")]
        public List<RequestCredentials> GetReceivedBadges()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            return requests.GetReceivedBadges(handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
        }

        [Authorize]
        [HttpGet("MyRequests")]
        public List<RequestCredentials> GetMyRequests()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            return requests.GetMyRequests(handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
        }
    }
}
