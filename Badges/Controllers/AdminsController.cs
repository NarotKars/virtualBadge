using Badges.Models;
using DataAccess;
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
    public class AdminsController : ControllerBase
    {
        private readonly IAdmins admins;

        public AdminsController(IAdmins admins) => this.admins = admins;

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("Quantity")]
        public ActionResult UpdateBadgeQuantity(int quantity)
        {
            admins.UpdateBadgeQuantity(quantity);
            return Ok("The quantity is successfully updated");
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("Day")]
        public ActionResult UpdateDayCount(int days)
        {
            admins.UpdateDayCount(days);
            return Ok("The number of days is successfully updated");
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("Badge")]
        public ActionResult AddBadgesToUser(APIBadge badge)
        {
            admins.AddBadgesToUser(badge.UserId, badge.Quantity);
            return Ok("The badges are successfully added");
        }
    }
}
