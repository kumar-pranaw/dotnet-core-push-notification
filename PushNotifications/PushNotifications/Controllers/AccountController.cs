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
                await _context.Users.AddAsync(userForRegisterDto);
                await _context.SaveChangesAsync();
                return Ok("User created successfully");
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthDto login)
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

            return Unauthorized();
        }

        [Authorize]
        [HttpGet("sendnotification")]
        public async Task<IActionResult> SendNotification()
        {
            try
            {
                var userId = HttpContext.User.Identity.Name;
                var getUserTokens = _context.UserTokens.Where(x => x.UserId ==  int.Parse(userId)).Select(x=> x.FToken).ToList();

               
               await _messagingClient.SendNotification(getUserTokens, "Hello Notification", "This is my first firebase notification");
                return Ok();    
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }


        private void SaveFirebaseToken(AuthDto user)
        {
            var userDetails = _context.Users.FirstOrDefault(x => x.Username.Equals(user.UserName) && x.Password.Equals(user.Password));

            if(userDetails != null && !string.IsNullOrWhiteSpace(user.FToken))
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

                    _context.UserTokens.AddAsync(token);
                    _context.SaveChangesAsync();
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
    }
}
