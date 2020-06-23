using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.CatalogModule.Web.JsonConverters;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.CatalogModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport
    {
        private IApplicationBuilder _appBuilder;

        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDynamicAssociationService, DynamicAssociationService>();
            serviceCollection.AddTransient<IDynamicAssociationSearchService, DynamicAssociationSearchService>();
            serviceCollection.AddTransient<IAuthorizationHandler, DynamicAssociationAuthorizationHandler>();
            serviceCollection.AddTransient<IDynamicAssociationEvaluator, DynamicAssociationEvaluator>();
            serviceCollection.AddTransient<IDynamicAssociationConditionSelector, DynamicAssociationConditionsSelector>();
            serviceCollection.AddTransient<DynamicAssociationSearchRequestBuilder>();
            serviceCollection.AddTransient<IDynamicAssociationConditionEvaluator, DynamicAssociationConditionEvaluator>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var mvcJsonOptions = appBuilder.ApplicationServices.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
            mvcJsonOptions.Value.SerializerSettings.Converters.Add(new PolymorphicDynamicAssociationsJsonConverter());

            //Register the resulting trees expressions into the AbstractFactory<IConditionTree> 
            foreach (var conditionTree in AbstractTypeFactory<DynamicAssociationRuleTreePrototype>.TryCreateInstance().Traverse<IConditionTree>(x => x.AvailableChildren))
            {
                AbstractTypeFactory<IConditionTree>.RegisterType(conditionTree.GetType());
            }
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<DynamicAssociationsExportImport>().DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<DynamicAssociationsExportImport>().DoImportAsync(inputStream, progressCallback, cancellationToken);
        }
    }
}
