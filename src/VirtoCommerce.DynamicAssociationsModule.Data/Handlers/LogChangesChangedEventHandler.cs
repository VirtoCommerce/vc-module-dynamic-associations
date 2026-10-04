using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Events;
using VirtoCommerce.DynamicAssociationsModule.Data.BackgroundJobs;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Handlers
{
    public class LogChangesChangedEventHandler : IEventHandler<AssociationChangedEvent>
    {
        private readonly IChangeLogService _changeLogService;

        public LogChangesChangedEventHandler(IChangeLogService changeLogService)
        {
            _changeLogService = changeLogService;
        }

        public Task Handle(AssociationChangedEvent message)
        {
            var logOperations = message
                .ChangedEntries
                .Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x))
                .ToArray();

            var payload = AbstractTypeFactory<LogEntityChangesJobPayload>.TryCreateInstance();
            payload.OperationLogs = logOperations;

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            return BackgroundJob.Enqueue<LogEntityChangesJobHandler>(payload);
        }

        /// <summary>
        /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
        /// New work goes through <see cref="LogEntityChangesJobHandler"/>; remove this once no such job
        /// can still be pending.
        /// </summary>
        // Signature is byte-identical on purpose: Hangfire persists a queued job as type name + method name +
        // parameter types + serialized args, so changing any of them would strand already-queued entries as Failed.
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses LogEntityChangesJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public virtual void LogEntityChangesInBackground(OperationLog[] operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs).GetAwaiter().GetResult();
        }
    }
}
