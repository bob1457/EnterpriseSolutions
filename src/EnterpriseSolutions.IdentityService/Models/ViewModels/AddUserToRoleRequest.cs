using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseSolutions.IdentityService.Models.ViewModels
{
    public class AddUserToRoleRequest
    {
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
