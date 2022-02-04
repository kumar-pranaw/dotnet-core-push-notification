using System.ComponentModel.DataAnnotations;

namespace PushNotifications.Dto
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FToken { get; set; }
        public string? DeviceToken { get; set; }

    }
}
