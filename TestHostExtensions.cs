﻿using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Host;

namespace Take.Blip.Client.Testing
{
    public static class TestHostExtensions
    {
        /// <summary>
        /// Register each object on the container as a service
        /// for each interface implemented
        /// </summary>
        /// <param name="host"></param>
        /// <param name="implementationOverrides">Object implementing some inteface to override Client's defaults</param>
        /// <returns></returns>
        public static Task<IServiceContainer> AddRegistrationAndStartAsync(this TestHost host, params object[] implementationOverrides)
        {
            return host.StartAsync(c =>
            {
                foreach (var registration in implementationOverrides
                                                .SelectMany(implementation => implementation.GetType().GetInterfaces()
                                                                                .Where(t => !t.Namespace.StartsWith("System"))
                                                                                .Select(i => new { Type = i, Implementation = implementation })))
                {

                    c.RegisterService(registration.Type, registration.Implementation);
                }
            });
        }

        public static async Task<Message> DeliverIncomingMessageAsnyc(this TestHost host, Node from, string plainContent)
        {
            var message = new Message
            {
                Id = EnvelopeId.NewId(),
                From = from,
                Content = new PlainText { Text = plainContent }
            };
            await host.DeliverIncomingMessageAsync(message);
            return message;
        }

        public static async Task WaitForConsumedAsync(this TestHost host)
        {
            var notification = await host.RetrieveOutgoingNotificationAsync();
            if (notification.Event != Event.Received) throw new InvalidOperationException($"Expected Received notification, but got {notification.Event}");
            notification = await host.RetrieveOutgoingNotificationAsync();
            if (notification.Event != Event.Consumed) throw new InvalidOperationException($"Expected Consumed notification, but got {notification.Event}");
        }

        public static async Task<IEnumerable<Message>> DeliverMessageAndConsumeResponseAsnyc(this TestHost host, Node from, string plainContent)
        {
            var result = new Message[2];
            result[0] = await host.DeliverIncomingMessageAsnyc(from, plainContent);
            await host.WaitForConsumedAsync();
            result[1] = await host.RetrieveOutgoingMessageAsync();
            return result;

        }

    }
}
