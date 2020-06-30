using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class ConditionProductCategory : ConditionTree
    {
        public string[] CategoryIds { get; set; }

        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;
            if (context is DynamicAssociationExpressionEvaluationContext evaluationContext)
            {
                result = evaluationContext.AreItemsInCategory(CategoryIds);
            }

            return result;
        }
    }
}
