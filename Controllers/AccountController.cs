using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NG_Core_Auth.Helpers;
using NG_Core_Auth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManger;
        // private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSetting _appSetting;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSetting> appSetting)
        {
            _userManger = userManager;
            // _signInManager = signInManager;
            _appSetting = appSetting.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistersViewModel model)
        {
            //Hold All Errors related to registeration 
            List<string> ErrorList = new List<string>();

            //Create User object From IdentityUser

            var User = new IdentityUser
            {
                Email = model.Email,
                UserName = model.UserName,

                //looking if anything change in username or password and updated
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManger.CreateAsync(User, model.Password);

            if (result.Succeeded)
            {
                //if user is Created Successful Added The default role(Customer) to new User
                await _userManger.AddToRoleAsync(User, "Customer");
                //sending Confirmation Email 

                //send information to angular app to validation ther user information
                return Ok(new { username = User.UserName, email = User.Email, Status = 1, message = "Registeration Successful" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    ErrorList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(ErrorList));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            //Check if User is Exist in database 
            var user = await _userManger.FindByNameAsync(model.UserName);
            var roles = await _userManger.GetRolesAsync(user);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSetting.Secret));
            double ExpireTokenTime = Convert.ToDouble(_appSetting.ExpaireTime);
            if (user != null && await _userManger.CheckPasswordAsync(user, model.Password))
            {
                //Token Handler to create token with 
                var tokenHandler = new JwtSecurityTokenHandler();

                //thing Add To Token

                var securityDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, model.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        //To Know When User LogIn
                        new Claim("LoggedOn", DateTime.Now.ToString()),

                    }),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSetting.Site,
                    Audience = _appSetting.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(ExpireTokenTime)
                };

                //Generate Token
                var token = tokenHandler.CreateToken(securityDescriptor);

                return Ok(new { Token = tokenHandler.WriteToken(token), expration = token.ValidTo, username = user.UserName, userRole = roles.FirstOrDefault() });
            }
            ModelState.AddModelError("", "Username Or Password Incorrect");
            return Unauthorized(new { LoginError = "Plz Check The Login Cerdential - Invaild UserName Or password " });
        }
    }
}
