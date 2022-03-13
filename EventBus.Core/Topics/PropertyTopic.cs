using SharedKernel;

namespace Infrastructure.EventBus.Core
{
    [Subscription(nameof(NotificationSubscription))]
    public sealed class PropertyTopic : IEventBusTopic
    {
    }
}
