using System;
using System.Collections.Generic;

namespace SharedKernel
{
    public sealed class EventBusMessageWrapper
    {
        public string Assembly { get; set; }
        public object Payload { get; set; }

        private EventBusMessageWrapper() { }
        public EventBusMessageWrapper(object payload) : this()
        {
            Payload = payload;
            Assembly = payload.GetType().AssemblyQualifiedName;
        }
    }

    public sealed class SubscriptionAttribute : Attribute
    {
        public IEnumerable<string> Names { get; }

        public SubscriptionAttribute(params string[] names) => Names = names;
    }

    public sealed class QueueAttribute : Attribute
    {
        public string Name { get; }
        public QueueAttribute(string name) => Name = name;
    }

    public sealed class TopicAttribute : Attribute
    {
        public string Name { get; }
        public TopicAttribute(string name) => Name = name;
    }
}
