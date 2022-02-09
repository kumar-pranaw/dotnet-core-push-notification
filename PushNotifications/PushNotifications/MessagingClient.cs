using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace PushNotifications
{
    public class MessagingClient : IMessagingClient
    {
        private readonly FirebaseMessaging messaging;
        public MessagingClient()
        {
            var app = FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile("serviceAccountKey.json").CreateScoped("https://www.googleapis.com/auth/firebase.messaging") });
            messaging = FirebaseMessaging.GetMessaging(app);
        }


        public async Task SendNotification(List<string> fcmTokens, string title, string body)
        {
            var result = await messaging.SendMulticastAsync(CreateNotification(fcmTokens, title, body));
            //do something with result
        }

        private MulticastMessage CreateNotification(List<string> registrationTokens, string title, string notificationBody)
        {
            return new MulticastMessage()
            {
                Tokens = registrationTokens,
                Data = new Dictionary<string, string>()
                     {
                     {"title", title},
                     {"body", notificationBody},
                     {"icon", "https://banner2.cleanpng.com/20191013/tat/transparent-ui-icon-notification-icon-bell-icon-5da3d1f3d32951.3009239215710172038649.jpg" }
                     },
            };
        }
    }
}
