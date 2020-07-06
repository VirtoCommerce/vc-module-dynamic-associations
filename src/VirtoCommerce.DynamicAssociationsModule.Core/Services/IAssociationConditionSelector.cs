using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IAssociationConditionSelector
    {
        Task<AssociationConditionEvaluationRequest> GetAssociationConditionAsync(AssociationEvaluationContext context, CatalogProduct product);
    }
}
