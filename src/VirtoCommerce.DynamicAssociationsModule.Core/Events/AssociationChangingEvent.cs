using System.Collections.Generic;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Events
{
    public class AssociationChangingEvent : GenericChangedEntryEvent<Association>
    {
        public AssociationChangingEvent(IEnumerable<GenericChangedEntry<Association>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
