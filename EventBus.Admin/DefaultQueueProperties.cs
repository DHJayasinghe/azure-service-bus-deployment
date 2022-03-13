using System;

namespace Infrastructure.EventBus.Admin
{
    internal class DefaultQueueProperties
    {
        public int MaxSizeInMegabytes { get; internal set; }
        public bool RequiresDuplicateDetection { get; internal set; }
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; internal set; }
        public bool RequiresSession { get; internal set; }
        public TimeSpan LockDuration { get; internal set; }
        public int MaxDeliveryCount { get; internal set; }
        public TimeSpan DefaultMessageTimeToLive { get; internal set; }
    }
}