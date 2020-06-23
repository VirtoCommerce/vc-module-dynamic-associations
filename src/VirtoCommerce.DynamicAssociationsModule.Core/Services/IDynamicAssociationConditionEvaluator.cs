using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IDynamicAssociationConditionEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationConditionAsync(DynamicAssociationConditionEvaluationRequest conditionRequest);
    }
}
