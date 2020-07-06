using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Search
{
    public interface IAssociationSearchService
    {
        Task<AssociationSearchResult> SearchAssociationsAsync(AssociationSearchCriteria criteria);
    }
}
