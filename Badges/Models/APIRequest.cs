using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Badges.Models
{
    public class APIRequest
    {
        public string RequesterId { get; set; }
        public string GiverId { get; set; }
        public int Quantity { get; set; }
    }
}
