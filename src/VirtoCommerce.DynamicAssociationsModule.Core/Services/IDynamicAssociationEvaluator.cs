using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IDynamicAssociationEvaluator
    {
        Task<string[]> EvaluateDynamicAssociationsAsync(DynamicAssociationEvaluationContext context);
    }
}
