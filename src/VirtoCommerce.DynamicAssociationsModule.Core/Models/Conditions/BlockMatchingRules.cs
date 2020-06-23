using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class BlockMatchingRules : BlockConditionAndOr
    {
        public BlockMatchingRules()
        {
            All = true;
        }
    }
}
