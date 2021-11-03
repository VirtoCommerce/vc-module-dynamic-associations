using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Models;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IAssociationConditionEvaluator
    {
        Task<AssociationConditionEvaluationResult> EvaluateAssociationConditionAsync(AssociationConditionEvaluationRequest conditionRequest);
    }
}
