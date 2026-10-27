using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Web.Controllers.Api;
using Xunit;

namespace VirtoCommerce.DynamicAssociationsModule.Tests
{
    public class AssociationsControllerContractTests
    {
        [Fact]
        public void PreviewDynamicAssociations_DeclaresTheTypeTheEvaluatorReturns()
        {
            // Swagger (and the generated API clients) take the response schema from the declared ActionResult<T>
            var returned = typeof(IAssociationConditionEvaluator)
                .GetMethod(nameof(IAssociationConditionEvaluator.EvaluateAssociationConditionAsync))!
                .ReturnType.GetGenericArguments()[0];

            var declared = typeof(AssociationsController)
                .GetMethod(nameof(AssociationsController.PreviewDynamicAssociations))!
                .ReturnType;

            Assert.Equal(typeof(Task<>).MakeGenericType(typeof(ActionResult<>).MakeGenericType(returned)), declared);
        }
    }
}
