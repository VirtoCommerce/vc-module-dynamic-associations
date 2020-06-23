using System.Collections.Generic;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Events
{
    public class DynamicAssociationChangedEvent : GenericChangedEntryEvent<DynamicAssociation>
    {
        public DynamicAssociationChangedEvent(IEnumerable<GenericChangedEntry<DynamicAssociation>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
