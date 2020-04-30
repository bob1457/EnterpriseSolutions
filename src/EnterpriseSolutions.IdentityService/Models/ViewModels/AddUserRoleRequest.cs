using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseSolutions.IdentityService.Models.ViewModels
{
    public class AddUserRoleRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
