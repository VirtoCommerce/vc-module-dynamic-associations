using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IDynamicAssociationConditionSelector
    {
        Task<DynamicAssociationConditionEvaluationRequest> GetDynamicAssociationConditionAsync(DynamicAssociationEvaluationContext context, CatalogProduct product);
    }
}
