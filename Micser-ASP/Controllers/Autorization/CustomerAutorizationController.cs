using tbank_back_web.Core.Identity.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ZooStores.Web.Area.Identity.Autorization.Models;

namespace ZooStores.Web.Area.Identity.Autorization
{
    [ApiController]
    [Route("accounts")]
    public class CustomerAutorizationController : ControllerBase
    {
        private readonly UserManager<BaseApplicationUser> _userManager;
        private readonly SignInManager<BaseApplicationUser> _signInManager;
		/// <summary>
		/// По дефолту новому пользователю присваивается роль Customer
		/// </summary>
		[AllowAnonymous]
        [HttpPost("registration")]
        public async Task<IActionResult> Registration(
            RegistrationRequestModel registrationRequest)
        {
            var newUser = new BaseApplicationUser
            {
				Email = registrationRequest.Login,
				UserName = registrationRequest.Login,

				Weight = registrationRequest.Weight,
				Heigth = registrationRequest.Heigth,
				Age = registrationRequest.Age,
				Gender = registrationRequest.Gender,
				ActivityLevel = registrationRequest.ActivityLevel
            };
            var res = await _userManager.CreateAsync(newUser, registrationRequest.Password);
            if (res.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "auth_customer");
                await _signInManager.SignInAsync(newUser, true);
                return Ok("Registration Successed");
            }
            else
            {
                foreach (var error in res.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

		[AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel requestModel)
        {
            var res = await _signInManager.PasswordSignInAsync(requestModel.login, requestModel.password, true, false);
            if (res.Succeeded)
            {
                return Ok("Login Succesed");
            }
            else
            {
                ModelState.AddModelError("Login", "Incorrect password");
            }
            return BadRequest(ModelState);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
        public CustomerAutorizationController(UserManager<BaseApplicationUser> userManager, SignInManager<BaseApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
    }
}
