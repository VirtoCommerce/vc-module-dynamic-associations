using VirtoCommerce.DynamicAssociationsModule.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace VirtoCommerce.DynamicAssociationsModule.Web.Controllers.Api
{
	[Route("api/VirtoCommerceDynamicAssociationsModule")]
	public class VirtoCommerceDynamicAssociationsModuleController : Controller
	{
		// GET: api/VirtoCommerceDynamicAssociationsModule
		/// <summary>
		/// Get message
		/// </summary>
		/// <remarks>Return "Hello world!" message</remarks>
		[HttpGet]
		[Route("")]
		[Authorize(ModuleConstants.Security.Permissions.Read)]
		public ActionResult<string> Get()
		{
			return Ok(new { result = "Hello world!" });
		}
	}
}
