namespace PushNotifications
{
    public interface IMessagingClient
    {
        Task SendNotification(string token, string title, string body);
    }
}
