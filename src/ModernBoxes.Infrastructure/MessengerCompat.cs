using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ModernBoxes.Infrastructure
{
    public static class MessengerCompat
    {
        public static WeakReferenceMessenger Default => WeakReferenceMessenger.Default;
    }

    public static class MessengerExtensions
    {
        public static void Send<TMessage>(this IMessenger messenger, TMessage message, string token)
        {
            var t = typeof(TMessage);
            if (t.IsValueType)
            {
                var method = typeof(MessengerExtensions).GetMethod(nameof(SendValue), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(t);
                method.Invoke(null, new object?[] { messenger, message, token });
            }
            else
            {
                var method = typeof(MessengerExtensions).GetMethod(nameof(SendRef), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(t);
                method.Invoke(null, new object?[] { messenger, message, token });
            }
        }

        private static void SendValue<TMessage>(IMessenger messenger, TMessage message, string token) where TMessage : struct
        {
            messenger.Send<ValueChangedMessage<TMessage>, string>(new ValueChangedMessage<TMessage>(message), token);
        }

        private static void SendRef<TMessage>(IMessenger messenger, TMessage message, string token) where TMessage : class
        {
            messenger.Send<TMessage, string>(message, token);
        }

        public static void Register<TMessage>(this IMessenger messenger, object recipient, string token, Action<TMessage> handler)
        {
            var t = typeof(TMessage);
            if (t.IsValueType)
            {
                var method = typeof(MessengerExtensions).GetMethod(nameof(RegisterValue), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(t);
                method.Invoke(null, new object?[] { messenger, recipient, token, handler });
            }
            else
            {
                var method = typeof(MessengerExtensions).GetMethod(nameof(RegisterRef), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(t);
                method.Invoke(null, new object?[] { messenger, recipient, token, handler });
            }
        }

        private static void RegisterValue<TMessage>(IMessenger messenger, object recipient, string token, Action<TMessage> handler) where TMessage : struct
        {
            messenger.Register<object, ValueChangedMessage<TMessage>, string>(recipient, token, (r, m) => handler(m.Value));
        }

        private static void RegisterRef<TMessage>(IMessenger messenger, object recipient, string token, Action<TMessage> handler) where TMessage : class
        {
            messenger.Register<object, TMessage, string>(recipient, token, (r, m) => handler(m));
        }
    }
}
