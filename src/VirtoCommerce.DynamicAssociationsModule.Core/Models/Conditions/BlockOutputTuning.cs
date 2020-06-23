using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class BlockOutputTuning : ConditionTree
    {
        public string Sort { get; set; }
        public int OutputLimit { get; set; } = 10;
    }
}
