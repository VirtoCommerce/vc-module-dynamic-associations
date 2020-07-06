using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Services
{
    public class AssociationConditionsSelector : IAssociationConditionSelector
    {
        private readonly IAssociationSearchService _associationSearchService;

        public AssociationConditionsSelector(IAssociationSearchService associationSearchService)
        {
            _associationSearchService = associationSearchService;
        }

        public virtual async Task<AssociationConditionEvaluationRequest> GetAssociationConditionAsync(AssociationEvaluationContext context, CatalogProduct product)
        {
            AssociationConditionEvaluationRequest result = null;

            var associationRules = (await _associationSearchService
                .SearchAssociationsAsync(new AssociationSearchCriteria
                {
                    Groups = new[] { context.Group },
                    StoreIds = new[] { context.StoreId },
                    Take = int.MaxValue,
                    SortInfos = { new SortInfo
                    {
                        SortColumn = nameof(Association.Priority),
                        SortDirection = SortDirection.Descending,
                    }},
                    IsActive = true,
                }))
                .Results;

            var expressionContext = AbstractTypeFactory<AssociationExpressionEvaluationContext>.TryCreateInstance();
            expressionContext.Products.Add(product);

            foreach (var associationRule in associationRules)
            {
                var matchingRule = associationRule.ExpressionTree.Children.OfType<BlockMatchingRules>().FirstOrDefault()
                    ?? throw new InvalidOperationException($"Matching rules block for \"{associationRule.Name}\" dynamic association rule is missing");

                if (matchingRule.IsSatisfiedBy(expressionContext))
                {
                    var resultRule = associationRule.ExpressionTree.Children.OfType<BlockResultingRules>().FirstOrDefault()
                        ?? throw new InvalidOperationException($"Resulting rules block for \"{associationRule.Name}\" dynamic association rule is missing");

                    var outputTuningBlock = associationRule.ExpressionTree.Children.OfType<BlockOutputTuning>().FirstOrDefault()
                        ?? throw new InvalidOperationException($"Output tuning block for \"{associationRule.Name}\" dynamic association rule is missing");

                    result = AbstractTypeFactory<AssociationConditionEvaluationRequest>.TryCreateInstance();
                    result.PropertyValues = resultRule.GetPropertyValues();
                    result.CatalogId = resultRule.GetCatalogId();
                    result.CategoryIds = resultRule.GetCategoryIds();
                    result.Sort = outputTuningBlock.Sort;
                    result.Skip = context.Skip;
                    result.Take = Math.Min(Math.Max(outputTuningBlock.OutputLimit - context.Skip, 0), context.Take);

                    break;
                }
            }

            return result;
        }
    }
}
