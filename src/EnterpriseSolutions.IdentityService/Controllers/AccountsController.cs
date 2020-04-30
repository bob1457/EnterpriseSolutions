using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseSolutions.IdentityService.Data;
using EnterpriseSolutions.IdentityService.Models;
using EnterpriseSolutions.IdentityService.Models.ViewModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseSolutions.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        //private readonly IUserRepository _userRepository;
        private readonly IEventService _events;

        public AccountsController(SignInManager<AppUser> signInManager, IIdentityServerInteractionService interaction, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppIdentityDbContext appIdentityDbContext, IEventService events/*, IUserRepository userRepository*/)
        {
            _signInManager = signInManager;
            _interaction = interaction;
            _userManager = userManager;
            _roleManager = roleManager;
            //_userRepository = userRepository;
            _events = events;
        }

        [Route("signup")]
        public async Task<IActionResult> Post([FromBody]SignupRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser { UserName = model.Email, FirstName = model.FirstName, LastName = model.LastName,  Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("userName", user.UserName));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("name", user.FirstName + " " + user.LastName));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("role", model.Role));
            //await _userRepository.InsertEntity(model.Role, user.Id, user.FullName);
            return Ok(new SignupResponse(user, model.Role));
        }
    }
}