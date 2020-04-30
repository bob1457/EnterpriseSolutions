using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseSolutions.IdentityService.Data;
using EnterpriseSolutions.IdentityService.Models;
using EnterpriseSolutions.IdentityService.Models.ViewModels;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
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
        public async Task<IActionResult> Signup([FromBody]SignupRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser { UserName = model.Email, FirstName = model.FirstName, LastName = model.LastName,  Email = model.Email };
            

            var result = await _userManager.CreateAsync(user, model.Password);
            

            if (!result.Succeeded) return BadRequest(result.Errors);
            
            await _userManager.AddToRoleAsync(user, model.Role);

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("userName", user.UserName));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("name", user.FirstName + " " + user.LastName));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("role", model.Role));
            //await _userRepository.InsertEntity(model.Role, user.Id, user.FullName);
            return Ok(new SignupResponse(user, model.Role));
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Route("signin")]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (ModelState.IsValid)
            {
                // validate username/password
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.FirstName));

                    // only set explicit expiration here if user chooses "remember me". 
                    // otherwise we rely upon expiration configured in cookie middleware.
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };

                    // issue authentication cookie with subject ID and username
                    await HttpContext.SignInAsync(user.Id, user.UserName, props);

                    //if (context != null)
                    //{
                    //    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    //    return Redirect(model.ReturnUrl);
                    //}

                    //// request for a local page
                    //if (Url.IsLocalUrl(model.ReturnUrl))
                    //{
                    //    return Redirect(model.ReturnUrl);
                    //}
                    //else if (string.IsNullOrEmpty(model.ReturnUrl))
                    //{
                    //    return Redirect("~/");
                    //}
                    //else
                    //{
                    //    // user might have clicked on a malicious link - should be logged
                    //    throw new Exception("invalid return URL");
                    //}
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error 
            var vm = new LoginViewModel
            {
                Username = model.Username,
                RememberLogin = model.RememberLogin
            };
            return Ok(vm); ;
        }


        [HttpPost]
        [Route("addrole")]
        public async Task<IActionResult> AddRole([FromBody]AddUserRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new AppRole();

            role.Name = request.Name;
            role.Descripton = request.Description;

            var result = await _roleManager.CreateAsync(role);
            

            return Ok(result);
        }

        [HttpPost]
        [Route("adduserrole")]
        public async Task<IActionResult> AddUserToRole([FromBody]AddUserToRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(request.UserName);
            var role = await _roleManager.FindByNameAsync(request.Role);

            if(user != null && role != null)
            {
                await _userManager.AddToRoleAsync(user, request.Role);
            }
            else
            {
                return Ok("Eitehr user or role does not exist!");
            }


            return Ok("Added successfully!");
        }
    }
}