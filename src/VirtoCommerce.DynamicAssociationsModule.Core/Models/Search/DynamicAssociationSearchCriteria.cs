using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Search
{
    public class DynamicAssociationSearchCriteria : SearchCriteriaBase
    {
        public string[] StoreIds { get; set; }
        public string[] Groups { get; set; }
        public bool? IsActive { get; set; }
    }
}
