using System.ComponentModel.DataAnnotations;

namespace PushNotifications.Dto
{
    public class HashLogs
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
        public string? TransactionId { get; set; }
        public string? ProductInfo { get; set; }
        public string? Amount { get; set; }
        public string? ApiKey { get; set; }
        public string? Salt { get; set; }
        public int UserId { get; set; }
        public string? Message { get; set; }
        public string? Hash { get; set; }
    }
}
