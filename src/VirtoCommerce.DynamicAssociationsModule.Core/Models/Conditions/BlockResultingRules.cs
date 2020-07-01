using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class BlockResultingRules : BlockConditionAndOr
    {
        public BlockResultingRules()
        {
            All = true;
        }

        public virtual Dictionary<string, string[]> GetPropertyValues() =>
            Children?.OfType<ConditionPropertyValues>()
                .FirstOrDefault()
                ?.GetPropertiesValues()
                .ToDictionary(x => x.Key, y => y.Value)
            ?? new Dictionary<string, string[]>();

        public virtual ICollection<string> GetCategoryIds() =>
            Children?.OfType<ConditionProductCategory>()
                .FirstOrDefault()
                ?.CategoryIds
            ?? Array.Empty<string>();

        public virtual string GetCatalogId() =>
            Children?.OfType<ConditionProductCategory>()
                .FirstOrDefault()
                ?.CatalogId
            ?? null;
    }
}
