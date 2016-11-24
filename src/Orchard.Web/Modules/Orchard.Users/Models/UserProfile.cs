using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Users.Models
{
    public class UserProfile
    {
        public string Name { get; set; }

        public string State { get; set; }
        public string City { get; set; }

        public string Country { get; set; }
        public string Postalcode { get; set; }

        public string Email { get; set; }
    }
}