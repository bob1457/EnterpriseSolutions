using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseSolutions.IdentityService.Models.ViewModels
{
    public class SignupResponse
    {
        public string Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public SignupResponse() { }

        public SignupResponse(AppUser user, string role)
        {
            Id = user.Id;
            Firstname = user.FirstName;
            Lastname = user.LastName;
            Email = user.Email;
            Role = role;
        }
    }
}
