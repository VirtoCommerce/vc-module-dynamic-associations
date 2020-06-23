using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model
{
    public class DynamicAssociationExpressionEvaluationContext : IEvaluationContext
    {
        public ICollection<CatalogProduct> Products { get; set; } = new List<CatalogProduct>();
    }
}
