using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using JuntoSegurosTest.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace JuntoSegurosTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("create")]
        public virtual async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result == null || !result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return BuildToken(model);
        }

        [HttpPost("login")]
        public virtual async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {
            var result = await _signInManager.PasswordSignInAsync(
                userInfo.Email,
                userInfo.Password,
                isPersistent: false,
                lockoutOnFailure: false
            );

            if (result == null || !result.Succeeded)
            {
                var loginFailed = "Usuário ou senha inválidos.";
                return BadRequest(loginFailed);
            }

            return BuildToken(userInfo);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("reset-password")]
        public virtual async Task<ActionResult<UserToken>> ResetPassword([FromBody] UserInfo userInfo)
        {
            var loginFailed = "Usuário ou senha inválidos.";
            var applicationUser = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (applicationUser == null)
            {
                return BadRequest(loginFailed);
            }

            var loginValid = await _userManager.CheckPasswordAsync(applicationUser, userInfo.Password);

            if (!loginValid)
            {
                return BadRequest(loginFailed);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);

            var result = await _userManager.ResetPasswordAsync(
                applicationUser,
                token,
                userInfo.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return BuildToken(userInfo);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("test-authentication")]
        public IActionResult TestAuthentication()
        {
            return Ok("Authentication is OK.");
        }

        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(_configuration.GetSection("Author").GetChildren());
        }

        private UserToken BuildToken(UserInfo userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds
            );

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
