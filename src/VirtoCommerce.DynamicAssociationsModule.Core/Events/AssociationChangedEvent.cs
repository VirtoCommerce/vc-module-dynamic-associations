using System.Collections.Generic;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Events
{
    public class AssociationChangedEvent : GenericChangedEntryEvent<Association>
    {
        public AssociationChangedEvent(IEnumerable<GenericChangedEntry<Association>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
