using BackendlessAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using BackendlessAPI.Messaging;
using Weborb.Client;

namespace NetConsoleSender
{

    class Program
    {
        static void Main(string[] args)
        {
            String appId = "A3D96FA2-7314-2543-FF66-0B60549D7300";
            String secretKey = "C52669F7-B0BC-4D8F-FF19-D120A11B7500";
            Backendless.URL = "http://api.backendless.com";
            try
            {
                Backendless.InitApp(appId, secretKey);
                var msgStatus = PublishMessageAsPushNotificationSync("default");
            }
            catch (Exception e)
            {
                Console.Write($"Error {e.Message}");
            }
        }

        static private MessageStatus PublishMessageAsPushNotificationSync(string message)
        {

            var publishOptions = new PublishOptions();
            publishOptions.Headers = new Dictionary<string, string>()
            {
                {"android-content-title", "Notification title for Android"},
                {"android-content-text", "Notification text for Android"},
                { "android-ticker-text", "ticker"}
            };


            DeliveryOptions deliveryOptions = new DeliveryOptions() {PushPolicy = PushPolicyEnum.ALSO, PushBroadcast = 1};

            MessageStatus messageStatus = Backendless.Messaging.Publish("default",
                message,
                publishOptions
                );
            if (messageStatus.ErrorMessage == null) {
                Console.WriteLine($"MessageStatus = {messageStatus.Status} Id {messageStatus.MessageId}");
                MessageStatus  updatedStatus = Backendless.Messaging.GetMessageStatus(messageStatus.MessageId);

                return messageStatus;
            }
            else {
                Console.WriteLine($"Server reported an error: {messageStatus.ErrorMessage}");
                return null;
            }
        }
    }
}
