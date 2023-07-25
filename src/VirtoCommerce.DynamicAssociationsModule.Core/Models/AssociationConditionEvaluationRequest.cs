using System.Collections.Generic;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model
{
    public class AssociationConditionEvaluationRequest
    {
        public string Keyword { get; set; }

        public string CatalogId { get; set; }

        public IList<string> CategoryIds { get; set; }

        public IDictionary<string, string[]> PropertyValues { get; set; }

        public string Sort { get; set; }

        public int Take { get; set; } = 20;

        public int Skip { get; set; } = 0;
    }
}
