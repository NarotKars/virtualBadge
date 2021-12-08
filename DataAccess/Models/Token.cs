using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Token
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}
