# azure-service-bus-deployment

Azure Service Bus topics,queues and subscriptions deployment using C# data annotations and Azure.Messaging.ServiceBus SDK.

Project structure:
1. EventBus.Core - This project defines how our service bus infrastructure is structured. Ex: Which queus are available, 
which topics are available and how many subscriptions are available for a topic using classes. 
Whatever the class placeholder classes implementing IEventBusQueue interface will consider as a queue which represent a service bus queue with given class name.
As same placeholder clases implementing IEventBusTopic interface will consider as a topic which represent a service bus topic with given class name. 
All subscriptions will be represent using IEventBusSubscription interface and their relationship with a given topic will represent on topic classes via special 
Subscription attribute.
2. EventBus.Admin - This project will examin above given standards in EventBus.Core project via c# reflection and will apply them to target Service Bus instance via
Azure.Messaging.ServiceBus SDK
