using SharedKernel;

namespace Infrastructure.EventBus.Core
{
    [Subscription(
        nameof(NotificationSubscription),
        nameof(SupportServiceSubscription),
        nameof(TimelineSubscription)
    )]
    public sealed class JobTopic : IEventBusTopic
    {
    }
}
