using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class BlockConditionRules : DynamicAssociationTree
    {
        public override bool IsSatisfiedBy(IEvaluationContext context)
        {
            var result = false;

            if (!Children.IsNullOrEmpty())
            {
                result = Children.All(ch => ch.IsSatisfiedBy(context));
            }

            return result;
        }
    }
}
