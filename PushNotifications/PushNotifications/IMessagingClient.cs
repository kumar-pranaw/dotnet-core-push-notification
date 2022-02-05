namespace PushNotifications
{
    public interface IMessagingClient
    {
        Task SendNotification(List<string> fcmTokens, string title, string body);
    }
}
