using System;
using System.Collections.Generic;
using System.Linq;

namespace viva.scraping.Models
{
    public class AuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
    }

    public class AuthResponse
    {
        public string accessToken { get; set; }
    }
}
