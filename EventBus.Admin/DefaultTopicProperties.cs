using System;

namespace Infrastructure.EventBus.Admin
{
    internal class DefaultTopicProperties
    {
        public bool RequiresDuplicateDetection { get; internal set; }
        public TimeSpan DuplicateDetectionHistoryTimeWindow { get; internal set; }
        public TimeSpan DefaultMessageTimeToLive { get; internal set; }
        public int MaxSizeInMegabytes { get; internal set; }
    }
}