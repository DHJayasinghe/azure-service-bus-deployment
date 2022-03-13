using Newtonsoft.Json;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.EventBus.Core
{
    public class EventBusCore
    {
        public static object MessageDeserializer(byte[] message)
        {
            var messageBody = System.Text.Encoding.UTF8.GetString(message);
            var unwrappedMessage = JsonConvert.DeserializeObject<EventBusMessageWrapper>(messageBody);
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(unwrappedMessage.Payload), Type.GetType(unwrappedMessage.Assembly));
        }

        /// <summary>
        /// Returns inherited topics
        /// </summary>
        public static IEnumerable<IEventBusTopic> GetTopics() => ReflectiveEnumerator.GetEnumerableOfType<IEventBusTopic>().ToList();

        /// <summary>
        /// Returns inherited queues
        /// </summary>
        public static IEnumerable<IEventBusQueue> GetQueues() => ReflectiveEnumerator.GetEnumerableOfType<IEventBusQueue>().ToList();

        /// <summary>
        /// Returns Subscriptions related to a Topic
        /// </summary>
        public static IEnumerable<string> GetSubscriptions(string topicName) =>
                ReflectiveEnumerator.GetEnumerableOfType<IEventBusTopic>()
                    .FirstOrDefault(d => d.GetType().Name == topicName)
                    ?.GetType().GetAttributeValue((SubscriptionAttribute subs) => subs.Names);
    }


    internal static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
        {
            List<T> objects = new List<T>();
            foreach (Type type in typeof(EventBusCore).Assembly.GetTypes()
                .Where(myType => myType.IsClass
                    && !myType.IsAbstract
                    && typeof(T).IsAssignableFrom(myType)))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            return objects;
        }

        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector
        ) where TAttribute : Attribute => type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() is TAttribute att
                ? valueSelector(att)
                : default;
    }
}
