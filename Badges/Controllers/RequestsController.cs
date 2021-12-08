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
using System.Linq;
using System.Threading.Tasks;

namespace Badges.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequests requests;
        private readonly IMapper mapper;

        public RequestsController(IRequests requests, IMapper mapper)
        {
            this.requests = requests;
            this.mapper = mapper;
        }

        [Authorize]
        [HttpPost("Create")]
        public ActionResult<int> CreateRequest(APIRequest request)
        {
            int newRequestId = 0;
            try
            {
                newRequestId = requests.CreateRequest(mapper.Map<Request>(request));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (newRequestId == 0)
                return BadRequest("An error occured");
            return newRequestId;
        }

        [Authorize]
        [HttpPut("Accept")]
        public ActionResult AcceptRequest(int id)
        {
            try
            {
                requests.AcceptRequest(id);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("The request is successfully accepted");
        }

        [Authorize]
        [HttpPut("Decline")]
        public ActionResult DeclineRequest(int id)
        {
            try
            {
                requests.DeclineRequest(id);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("The request is successfully declined");
        }

        [Authorize]
        [HttpGet("Received")]
        public ActionResult<List<RequestCredentials>> GetReceivedBadges()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            List<RequestCredentials> receivedBadges;
            try
            {
                receivedBadges = requests.GetReceivedBadges(handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return receivedBadges;
        }

        [Authorize]
        [HttpGet("myRequests")]
        public ActionResult<List<RequestCredentials>> GetMyRequests()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            List<RequestCredentials> myRequests;
            try
            {
                myRequests = requests.GetMyRequests(handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return myRequests;
        }

        [Authorize]
        [HttpGet("pending")]
        public ActionResult<List<RequestCredentials>> GetPendingRequests()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var handler = new JwtSecurityTokenHandler();
            List<RequestCredentials> pendingRequests;
            try
            {
                pendingRequests = requests.GetPendingRequests(handler.ReadJwtToken(accessToken.Split(" ")[1]).Subject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return pendingRequests;
        }
    }
}
