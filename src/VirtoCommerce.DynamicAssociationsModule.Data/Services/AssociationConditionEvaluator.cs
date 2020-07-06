using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Services
{
    public class AssociationConditionEvaluator : IAssociationConditionEvaluator
    {
        private readonly AssociationSearchRequestBuilder _requestBuilder;
        private readonly ISearchProvider _searchProvider;

        public AssociationConditionEvaluator(
            AssociationSearchRequestBuilder requestBuilder,
            ISearchProvider searchProvider
            )
        {
            _requestBuilder = requestBuilder;
            _searchProvider = searchProvider;
        }

        public async Task<string[]> EvaluateAssociationConditionAsync(AssociationConditionEvaluationRequest conditionRequest)
        {
            _requestBuilder
                .AddOutlineSearch(conditionRequest.CatalogId, conditionRequest.CategoryIds)
                .AddPropertySearch(conditionRequest.PropertyValues)
                .AddKeywordSearch(conditionRequest.Keyword)
                .AddSortInfo(conditionRequest.Sort)
                .WithPaging(conditionRequest.Skip, conditionRequest.Take);

            var searchResult = await _searchProvider.SearchAsync(KnownDocumentTypes.Product, _requestBuilder.Build());

            return searchResult.Documents.Select(x => x.Id).ToArray();
        }
    }
}
