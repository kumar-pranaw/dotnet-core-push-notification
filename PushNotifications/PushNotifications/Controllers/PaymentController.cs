using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PushNotifications.Dto;
using System.Security.Cryptography;
using System.Text;

namespace PushNotifications.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private const string ApiKey = "Isa01Q";
        private const string Salt = "aSkRQ4sLjhWfcQKeMVbuYfiH94ohjmb9";
        private readonly DataContext _context;
        public PaymentController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("generateHash")]
        public async Task<IActionResult> GenerateHash([FromBody] HashDto hashDto)
        {
            var userId = HttpContext.User.Identity?.Name;

            var concatenatedString =  String.Join("", hashDto.TransactionId, hashDto.FirstName, hashDto.ProductInfo, hashDto.Email, hashDto.Amount, ApiKey, Salt);

            var generatedHash = ComputeSha256Hash(concatenatedString);
            HashLogs hashLogs = new HashLogs()
            {
                FirstName = hashDto.FirstName,
                Amount = hashDto.Amount,
                Email = hashDto.Email,
                ApiKey = ApiKey,
                Salt = Salt,
                Message = "Hash Generated for the user successfully",
                Hash = generatedHash,
                ProductInfo = hashDto.ProductInfo,
                TransactionId = hashDto.TransactionId,
                UserId = int.Parse(userId),
            };

            await _context.HashLogs.AddAsync(hashLogs);
            await _context.SaveChangesAsync();

            return Ok(new { message = $@"Hash generated successfully for the user {hashDto.FirstName}", hash = generatedHash });

        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}

