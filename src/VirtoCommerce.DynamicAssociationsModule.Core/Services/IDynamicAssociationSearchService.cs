using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Search
{
    public interface IDynamicAssociationSearchService
    {
        Task<DynamicAssociationSearchResult> SearchDynamicAssociationsAsync(DynamicAssociationSearchCriteria criteria);
    }
}
