using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IAssociationEvaluator
    {
        Task<string[]> EvaluateAssociationsAsync(AssociationEvaluationContext context);
    }
}
