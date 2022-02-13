using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using PushNotifications.Dto;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;

namespace PushNotifications.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DataContext _context;
        private readonly IMessagingClient _messagingClient;
        public AccountController(IConfiguration config, DataContext context, IMessagingClient messagingClient)
        {
            _config = config;
            _context = context;
            _messagingClient = messagingClient;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User userForRegisterDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                var checkUserName = _context.Users.Any(x => x.Username == userForRegisterDto.Username);
                if (checkUserName)
                {
                    return BadRequest(new { message = "This user already exists" });
                }
                await _context.Users.AddAsync(userForRegisterDto);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User created successfully" });
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthDto login)
        {
            try
            {
                var result = _context.Users.Any(x => x.Username.Equals(login.UserName) && x.Password.Equals(login.Password));

                if (result)
                {
                    var user = _context.Users.FirstOrDefault(x => x.Username.Equals(login.UserName) && x.Password.Equals(login.Password));

                    SaveFirebaseToken(login);

                    return Ok(new
                    {
                        token = GenerateJwtToken(user).Result,
                        user = user
                    });
                }

                return Unauthorized(new { message = "Unauthorized user" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet("sendnotification/{userId}")]
        public async Task<IActionResult> SendNotification(string userId)
        {
            try
            {
                //var userId = HttpContext.User.Identity.Name;
                var getUserTokens = _context.UserTokens.Where(x => x.UserId == int.Parse(userId)).Select(x => x.FToken).ToList();


                var result = await _messagingClient.SendNotification(getUserTokens, "Hello Notification", "This is my first firebase notification");


                if (result.Responses.Count == 0)
                {
                    return Ok(new { message = "No active ftoken found for this user" });
                }
                else
                {
                    // Remove the token from db when isSuccess is false
                    for (int i = 0; i < result.Responses.Count; i++)
                    {
                        if (!result.Responses[i].IsSuccess)
                        {
                            var tokenToRemove = _context.UserTokens.Where(x => x.FToken == getUserTokens[i]).FirstOrDefault();
                            _context.UserTokens.Remove(tokenToRemove);
                            await _context.SaveChangesAsync();
                        }
                    }
                    return Ok(new { message = "notification sent successfully"});
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize]
        [HttpDelete("logOut")]
        public async Task<IActionResult> LogOut(string fToken)
        {
            var userId = HttpContext.User.Identity.Name;
            var getUserTokens = _context.UserTokens.Where(x => x.UserId == int.Parse(userId) && x.FToken == fToken).FirstOrDefault();

            _context.UserTokens.Remove(getUserTokens);
            await _context.SaveChangesAsync();

            return Ok(new { message = "user successfully logged out"});
        }

        #region Private helper methods
        private void SaveFirebaseToken(AuthDto user)
        {
            var userDetails = _context.Users.FirstOrDefault(x => x.Username.Equals(user.UserName) && x.Password.Equals(user.Password));

            if (userDetails != null && !string.IsNullOrWhiteSpace(user.FToken))
            {
                var checkTokenExists = _context.UserTokens.Any(x => (x.UserId == userDetails.Id && x.FToken == user.FToken));

                if (!checkTokenExists)
                {
                    UserToken token = new UserToken()
                    {
                        DeviceToken = user.FToken,
                        FToken = user.FToken,
                        UserId = userDetails.Id
                    };

                    _context.UserTokens.Add(token);
                    _context.SaveChanges();
                }
            }
        }

        private async Task<string> GenerateJwtToken(User? user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        #endregion

    }
}
