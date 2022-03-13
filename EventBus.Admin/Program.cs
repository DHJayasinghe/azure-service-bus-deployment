using Azure.Messaging.ServiceBus.Administration;
using Infrastructure.EventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.EventBus.Admin
{
    class Program
    {
        private static string ServiceBusConnection = "";
        static async Task Main(string[] args)
        {
            HashSet<string> queues = new HashSet<string>();
            Dictionary<string, IEnumerable<string>> topics = new Dictionary<string, IEnumerable<string>>();

            EventBusCore.GetTopics().ToList().ForEach(d =>
            {
                string name = d.GetType().Name;
                if (!string.IsNullOrEmpty(name) && !topics.ContainsKey(name))
                {
                    topics.Add(name, EventBusCore.GetSubscriptions(name));
                    return;
                }
            });

            EventBusCore.GetQueues().ToList().ForEach(d =>
            {
                string name = d.GetType().Name;
                if (!string.IsNullOrEmpty(name) && !queues.Contains(name))
                {
                    queues.Add(name);
                    return;
                }
            });

            var adminClient = new ServiceBusAdministrationClient(ServiceBusConnection);
            var cancellationToken = new CancellationToken();
            var queuesCreateTasks = queues.Select(queue => CreateQueueIfNotExistAsync(adminClient, queue, cancellationToken));
            var topicsCreateTasks = topics.Keys.Select(topic => CreateTopicIfNotExistAsync(adminClient, topic, cancellationToken));
            await Task.WhenAll(queuesCreateTasks);
            await Task.WhenAll(topicsCreateTasks);

            var subscriptionsCreateTasks = topics.Where(d => d.Value != null)
                .SelectMany(topic => topic.Value
                    .Select(subscription => CreateSubscriptionIfNotExistAsync(
                            adminClient, name: subscription, topicName: topic.Key, cancellationToken)));
            await Task.WhenAll(subscriptionsCreateTasks);
        }

        private static readonly DefaultQueueProperties DefaultQueueProperties =
            new DefaultQueueProperties
            {
                MaxSizeInMegabytes = 1024, // queue size: 1 GB default, max 5 GB 
                RequiresDuplicateDetection = true, // Based on MessageId property
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10), // default 10min, max 7 days
                RequiresSession = false, // FIFO delivery guarantee
                LockDuration = TimeSpan.FromSeconds(30), // default 30sec, max 5min
                MaxDeliveryCount = 10,// retry attempt count, default 10, max 2000
                DefaultMessageTimeToLive = TimeSpan.FromDays(30) // how long a message gonna stay in the queue without processing, before move to DLQ
            };
        private static async Task<bool> CreateQueueIfNotExistAsync(
            ServiceBusAdministrationClient adminClient,
            string name,
            CancellationToken cancellationToken)
        {
            if (await adminClient.QueueExistsAsync(name, cancellationToken))
            {
                Console.WriteLine($"Queue {name} creation - Already Exist");

                try
                {
                    var queueProperties = (await adminClient.GetQueueAsync(name, cancellationToken)).Value;

                    queueProperties.MaxSizeInMegabytes = DefaultQueueProperties.MaxSizeInMegabytes;
                    queueProperties.DuplicateDetectionHistoryTimeWindow = DefaultQueueProperties.DuplicateDetectionHistoryTimeWindow;
                    queueProperties.LockDuration = DefaultQueueProperties.LockDuration;
                    queueProperties.MaxDeliveryCount = DefaultQueueProperties.MaxDeliveryCount;
                    queueProperties.DefaultMessageTimeToLive = DefaultQueueProperties.DefaultMessageTimeToLive;
                    queueProperties.MaxSizeInMegabytes = DefaultTopicProperties.MaxSizeInMegabytes;
                    queueProperties.DuplicateDetectionHistoryTimeWindow = DefaultTopicProperties.DuplicateDetectionHistoryTimeWindow;
                    queueProperties.DefaultMessageTimeToLive = DefaultTopicProperties.DefaultMessageTimeToLive;

                    await adminClient.UpdateQueueAsync(queueProperties, cancellationToken);

                    Console.WriteLine($"Queue {name} update - Success");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Queue {name} update - Failed: " + ex.Message);
                    return false;
                }
            }

            try
            {
                await adminClient.CreateQueueAsync(new CreateQueueOptions(name)
                {
                    MaxSizeInMegabytes = DefaultQueueProperties.MaxSizeInMegabytes,
                    RequiresDuplicateDetection = DefaultQueueProperties.RequiresDuplicateDetection,
                    DuplicateDetectionHistoryTimeWindow = DefaultQueueProperties.DuplicateDetectionHistoryTimeWindow,
                    RequiresSession = DefaultQueueProperties.RequiresSession,
                    LockDuration = DefaultQueueProperties.LockDuration,
                    MaxDeliveryCount = DefaultQueueProperties.MaxDeliveryCount,
                    DefaultMessageTimeToLive = DefaultQueueProperties.DefaultMessageTimeToLive
                }, cancellationToken);

                Console.WriteLine($"Queue {name} creation - Success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Queue {name} creation - Failed: " + ex.Message);
                return false;
            }
        }

        private static readonly DefaultTopicProperties DefaultTopicProperties =
            new DefaultTopicProperties
            {
                MaxSizeInMegabytes = 1024, // Topic size: 1 GB default, max 5 GB 
                RequiresDuplicateDetection = true, // Based on MessageId property
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10), // default 10min, max 7 days
                DefaultMessageTimeToLive = TimeSpan.FromDays(30) // how long a message gonna stay in the topic without processing, before move to DLQ
            };
        private static async Task<bool> CreateTopicIfNotExistAsync(
            ServiceBusAdministrationClient adminClient,
            string name,
            CancellationToken cancellationToken)
        {
            if (await adminClient.TopicExistsAsync(name, cancellationToken))
            {
                Console.WriteLine($"Topic {name} creation - Already Exist");

                try
                {
                    var topicProperties = (await adminClient.GetTopicAsync(name, cancellationToken)).Value;
                    topicProperties.MaxSizeInMegabytes = DefaultTopicProperties.MaxSizeInMegabytes;
                    topicProperties.DuplicateDetectionHistoryTimeWindow = DefaultTopicProperties.DuplicateDetectionHistoryTimeWindow;
                    topicProperties.DefaultMessageTimeToLive = DefaultTopicProperties.DefaultMessageTimeToLive;

                    await adminClient.UpdateTopicAsync(topicProperties, cancellationToken);

                    Console.WriteLine($"Topic {name} update - Success");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Topic {name} update - Failed: " + ex.Message);
                    return false;
                }
            }

            try
            {
                await adminClient.CreateTopicAsync(new CreateTopicOptions(name)
                {
                    MaxSizeInMegabytes = DefaultTopicProperties.MaxSizeInMegabytes,
                    RequiresDuplicateDetection = DefaultTopicProperties.RequiresDuplicateDetection,
                    DuplicateDetectionHistoryTimeWindow = DefaultTopicProperties.DuplicateDetectionHistoryTimeWindow,
                    DefaultMessageTimeToLive = DefaultTopicProperties.DefaultMessageTimeToLive
                }, cancellationToken);

                Console.WriteLine($"Topic {name} creation - Success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Topic {name} creation - Failed: " + ex.Message);
                return false;
            }
        }

        private static readonly DefaultSubscriptionProperties DefaultSubscriptionProperties =
            new DefaultSubscriptionProperties
            {
                RequiresSession = false, // FIFO delivery guarantee
                LockDuration = TimeSpan.FromSeconds(30), // default 30sec, max 5min
                MaxDeliveryCount = 10,// retry attempt count, default 10, max 2000
                DefaultMessageTimeToLive = TimeSpan.FromDays(30) // how long a message gonna stay in the queue without processing, before move to DLQ
            };
        private static async Task<bool> CreateSubscriptionIfNotExistAsync(
            ServiceBusAdministrationClient adminClient,
            string name,
            string topicName,
            CancellationToken cancellationToken)
        {
            if (await adminClient.SubscriptionExistsAsync(topicName, name, cancellationToken))
            {
                Console.WriteLine($"Subscription {name} creation - Already Exist");

                try
                {
                    var subscriptionProperties = (await adminClient.GetSubscriptionAsync(topicName, name, cancellationToken)).Value;

                    subscriptionProperties.LockDuration = DefaultQueueProperties.LockDuration;
                    subscriptionProperties.MaxDeliveryCount = DefaultQueueProperties.MaxDeliveryCount;
                    subscriptionProperties.DefaultMessageTimeToLive = DefaultQueueProperties.DefaultMessageTimeToLive;
                    subscriptionProperties.DefaultMessageTimeToLive = DefaultTopicProperties.DefaultMessageTimeToLive;

                    await adminClient.UpdateSubscriptionAsync(subscriptionProperties, cancellationToken);

                    Console.WriteLine($"Subscription {name} update - Success");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Subscription {name} update - Failed: " + ex.Message);
                    return false;
                }
            }

            try
            {
                await adminClient.CreateSubscriptionAsync(new CreateSubscriptionOptions(topicName, subscriptionName: name)
                {
                    RequiresSession = DefaultSubscriptionProperties.RequiresSession,
                    LockDuration = DefaultSubscriptionProperties.LockDuration,
                    MaxDeliveryCount = DefaultSubscriptionProperties.MaxDeliveryCount,
                    DefaultMessageTimeToLive = DefaultSubscriptionProperties.DefaultMessageTimeToLive
                });

                Console.WriteLine($"Subscription {name} creation - Success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subscription {name} creation - Failed: " + ex.Message);
                return false;
            }
        }
    }
}
