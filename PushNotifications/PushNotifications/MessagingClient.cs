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


        public async Task SendNotification(string token, string title, string body)
        {
            var result = await messaging.SendAsync(CreateNotification(title, body, token));
            //do something with result
        }

        private Message CreateNotification(string title, string notificationBody, string token)
        {
            return new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = title
                }
            };
        }
    }
}
