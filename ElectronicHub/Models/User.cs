using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicHub.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string ImageBase64 { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}