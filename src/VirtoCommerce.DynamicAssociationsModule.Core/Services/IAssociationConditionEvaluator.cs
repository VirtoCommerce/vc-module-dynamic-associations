using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IAssociationConditionEvaluator
    {
        Task<string[]> EvaluateAssociationConditionAsync(AssociationConditionEvaluationRequest conditionRequest);
    }
}
