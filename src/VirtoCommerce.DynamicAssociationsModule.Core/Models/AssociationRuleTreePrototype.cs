using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model
{
    /// <summary>
    /// Represents the prototype for rule tree <see cref="AssociationRuleTree"/> containing the list of available rules for building a rule tree in UI
    /// </summary>
    public class AssociationRuleTreePrototype : ConditionTree
    {
        public AssociationRuleTreePrototype()
        {
            var matchingRules = new BlockMatchingRules()
                .WithAvailConditions(
                    new ConditionProductCategory(),
                    new ConditionPropertyValues()
                );
            var resultingRules = new BlockResultingRules()
                .WithAvailConditions(
                    new ConditionProductCategory(),
                    new ConditionPropertyValues()
                );
            var outputTuning = new BlockOutputTuning();

            WithAvailConditions(
                matchingRules,
                resultingRules,
                outputTuning
            );
            WithChildrens(
                matchingRules,
                resultingRules,
                outputTuning
            );
        }
    }
}
