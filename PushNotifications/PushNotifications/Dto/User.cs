using System.ComponentModel.DataAnnotations;

namespace PushNotifications.Controllers
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public DateTime Created { get; set; }
        public User()
        {
            Created = DateTime.Now;
        }
    }
}