using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.ExportImport;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;
using VirtoCommerce.DynamicAssociationsModule.Data.Search;
using VirtoCommerce.DynamicAssociationsModule.Data.Services;
using VirtoCommerce.DynamicAssociationsModule.Web.Authorization;
using VirtoCommerce.DynamicAssociationsModule.Web.JsonConverters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Data.Extensions;

namespace VirtoCommerce.DynamicAssociationsModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport
    {
        private IApplicationBuilder _appBuilder;

        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {

            var configuration = serviceCollection.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("VirtoCommerce.DynamicAssociationsModule") ?? configuration.GetConnectionString("VirtoCommerce");


            serviceCollection.AddTransient<IDynamicAssociationService, DynamicAssociationService>();
            serviceCollection.AddTransient<IDynamicAssociationsRepository, DynamicAssociationsRepository>();
            serviceCollection.AddTransient<IDynamicAssociationSearchService, DynamicAssociationSearchService>();
            serviceCollection.AddTransient<IAuthorizationHandler, DynamicAssociationAuthorizationHandler>();
            serviceCollection.AddTransient<IDynamicAssociationEvaluator, DynamicAssociationEvaluator>();
            serviceCollection.AddTransient<IDynamicAssociationConditionSelector, DynamicAssociationConditionsSelector>();
            serviceCollection.AddTransient<DynamicAssociationSearchRequestBuilder>();
            serviceCollection.AddTransient<IDynamicAssociationConditionEvaluator, DynamicAssociationConditionEvaluator>();

            serviceCollection.AddDbContext<DynamicAssociationsModuleDbContext>(options => options.UseSqlServer(connectionString));
            serviceCollection.AddTransient<Func<IDynamicAssociationsRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IDynamicAssociationsRepository>());
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var mvcJsonOptions = appBuilder.ApplicationServices.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
            mvcJsonOptions.Value.SerializerSettings.Converters.Add(new PolymorphicDynamicAssociationsJsonConverter());
            mvcJsonOptions.Value.SerializerSettings.Converters.Add(new DynamicAssociationTreeJsonConverter());

            //Register the resulting trees expressions into the AbstractFactory<IConditionTree> 
            foreach (var conditionTree in AbstractTypeFactory<DynamicAssociationRuleTreePrototype>.TryCreateInstance().Traverse<IDynamicAssociationTree>(x => x.AvailableChildren.OfType<IDynamicAssociationTree>()))
            {
                AbstractTypeFactory<IDynamicAssociationTree>.RegisterType(conditionTree.GetType());
            }

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<DynamicAssociationsModuleDbContext>();
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
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
