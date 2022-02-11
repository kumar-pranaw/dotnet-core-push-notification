using FirebaseAdmin.Messaging;

namespace PushNotifications
{
    public interface IMessagingClient
    {
        Task<BatchResponse> SendNotification(List<string> fcmTokens, string title, string body);
    }
}
