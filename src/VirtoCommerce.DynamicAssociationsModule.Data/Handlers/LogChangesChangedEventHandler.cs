using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.DynamicAssociationsModule.Core.Events;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

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

            BackgroundJob.Enqueue(() => LogEntityChangesInBackground(logOperations));

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(10)]
        public virtual void LogEntityChangesInBackground(OperationLog[] operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs).GetAwaiter().GetResult();
        }
    }
}
