using Microsoft.EntityFrameworkCore;
using PushNotifications.Controllers;
using PushNotifications.Dto;

namespace PushNotifications
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
    }
}
