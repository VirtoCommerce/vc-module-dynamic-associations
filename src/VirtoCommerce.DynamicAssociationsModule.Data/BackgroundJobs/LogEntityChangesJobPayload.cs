using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.DynamicAssociationsModule.Data.BackgroundJobs
{
    /// <summary>
    /// Payload of <see cref="LogEntityChangesJobHandler"/>: the dynamic association change-log entries to persist.
    /// </summary>
    public class LogEntityChangesJobPayload
    {
        public OperationLog[] OperationLogs { get; set; }
    }
}
