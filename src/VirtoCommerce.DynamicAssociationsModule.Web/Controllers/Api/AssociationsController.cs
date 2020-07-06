using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Common;
using CatalogModuleConstants = VirtoCommerce.CatalogModule.Core.ModuleConstants;

namespace VirtoCommerce.DynamicAssociationsModule.Web.Controllers.Api
{
    [Route("api/dynamicassociations")]
    [Authorize]
    public class AssociationsController : Controller
    {
        private readonly IAssociationSearchService _associationSearchService;
        private readonly IAssociationService _associationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAssociationEvaluator _associationEvaluator;
        private readonly IAssociationConditionEvaluator _associationConditionEvaluator;

        public AssociationsController(
            IAssociationSearchService associationSearchService,
            IAssociationService associationService,
            IAuthorizationService authorizationService,
            IAssociationEvaluator associationEvaluator,
            IAssociationConditionEvaluator associationConditionEvaluator)
        {
            _associationSearchService = associationSearchService;
            _associationService = associationService;
            _authorizationService = authorizationService;
            _associationEvaluator = associationEvaluator;
            _associationConditionEvaluator = associationConditionEvaluator;
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<AssociationSearchResult>> SearchAssociation([FromBody] AssociationSearchCriteria criteria)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                criteria,
                new AssociationAuthorizationRequirement(CatalogModuleConstants.Security.Permissions.Read)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var result = await _associationSearchService.SearchAssociationsAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Gets the dynamic association by Id
        /// </summary>
        /// <param name="id">Association id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Association>> GetAssociationById(string id)
        {
            var result = (await _associationService.GetByIdsAsync(new[] { id })).FirstOrDefault();

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                result,
                new AssociationAuthorizationRequirement(CatalogModuleConstants.Security.Permissions.Read)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            result?.ExpressionTree?.MergeFromPrototype(AbstractTypeFactory<AssociationRuleTreePrototype>.TryCreateInstance());

            return Ok(result);
        }

        /// <summary>
        /// Get new dynamic association object 
        /// </summary>
        /// <remarks>Return a new dynamic association object with populated dynamic expression tree</remarks>
        [HttpGet]
        [Route("new")]
        [Authorize(CatalogModuleConstants.Security.Permissions.Create)]
        public ActionResult<Association> GetNewAssociation()
        {
            var result = AbstractTypeFactory<Association>.TryCreateInstance();

            result.ExpressionTree = AbstractTypeFactory<AssociationRuleTree>.TryCreateInstance();
            result.ExpressionTree.MergeFromPrototype(AbstractTypeFactory<AssociationRuleTreePrototype>.TryCreateInstance());
            result.IsActive = true;

            return Ok(result);
        }

        /// <summary>
        /// Create/Update associations.
        /// </summary>
        /// <param name="associations">The dynamic association rules.</param>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<Association[]>> SaveAssociations([FromBody] Association[] associations)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                associations,
                new AssociationAuthorizationRequirement(CatalogModuleConstants.Security.Permissions.Update)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            if (!associations.IsNullOrEmpty())
            {
                await _associationService.SaveChangesAsync(associations);
            }

            return Ok(associations);
        }

        /// <summary>
        /// Deletes association rule by id.
        /// </summary>
        /// <remarks>Deletes association rule by id</remarks>
        /// <param name="ids">association ids.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteAssociation([FromQuery] string[] ids)
        {
            var dynamicAssociations = await _associationService.GetByIdsAsync(ids);
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                dynamicAssociations,
                new AssociationAuthorizationRequirement(CatalogModuleConstants.Security.Permissions.Delete)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            await _associationService.DeleteAsync(ids);

            return NoContent();
        }

        /// <summary>
        /// Evaluate dynamic associations.
        /// </summary>
        /// <param name="context">Search context</param>
        /// <returns>Associated products ids.</returns>
        [HttpPost]
        [Route("evaluate")]
        public async Task<ActionResult<string[]>> EvaluateDynamicAssociations([FromBody] AssociationEvaluationContext context)
        {
            ValidateParameters(context);

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                context,
                new AssociationAuthorizationRequirement(CatalogModuleConstants.Security.Permissions.Read)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var result = await _associationEvaluator.EvaluateAssociationsAsync(context);

            return Ok(result);
        }

        [HttpPost]
        [Route("preview")]
        public async Task<ActionResult<string[]>> PreviewDynamicAssociations([FromBody] AssociationConditionEvaluationRequest conditionRequest)
        {
            ValidateParameters(conditionRequest);

            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                conditionRequest,
                new AssociationAuthorizationRequirement(CatalogModuleConstants.Security.Permissions.Read)
                );

            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var result = await _associationConditionEvaluator.EvaluateAssociationConditionAsync(conditionRequest);

            return Ok(result);
        }

        private static void ValidateParameters(AssociationConditionEvaluationRequest conditionRequest)
        {
            if (conditionRequest == null)
            {
                throw new ArgumentNullException(nameof(conditionRequest));
            }
        }

        private static void ValidateParameters(AssociationEvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }
    }
}
