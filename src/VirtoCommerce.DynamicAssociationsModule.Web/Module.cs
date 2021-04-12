using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Events;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Search;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Data.ExportImport;
using VirtoCommerce.DynamicAssociationsModule.Data.Handlers;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;
using VirtoCommerce.DynamicAssociationsModule.Data.Search;
using VirtoCommerce.DynamicAssociationsModule.Data.Services;
using VirtoCommerce.DynamicAssociationsModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;

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


            serviceCollection.AddTransient<IAssociationService, AssociationService>();
            serviceCollection.AddTransient<IAssociationsRepository, AssociationsRepository>();
            serviceCollection.AddTransient<IAssociationSearchService, AssociationSearchService>();
            serviceCollection.AddTransient<IAuthorizationHandler, AssociationAuthorizationHandler>();
            serviceCollection.AddTransient<IAssociationEvaluator, AssociationEvaluator>();
            serviceCollection.AddTransient<IAssociationConditionSelector, AssociationConditionsSelector>();
            serviceCollection.AddTransient<AssociationSearchRequestBuilder>();
            serviceCollection.AddTransient<IAssociationConditionEvaluator, AssociationConditionEvaluator>();
            serviceCollection.AddTransient<AssociationsExportImport>();

            serviceCollection.AddTransient<LogChangesChangedEventHandler>();

            serviceCollection.AddDbContext<AssociationsModuleDbContext>(options => options.UseSqlServer(connectionString));
            serviceCollection.AddTransient<Func<IAssociationsRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IAssociationsRepository>());
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            //Register the resulting trees expressions into the AbstractFactory<IConditionTree> 
            foreach (var conditionType in AbstractTypeFactory<AssociationRuleTreePrototype>.TryCreateInstance()
                .Traverse<IConditionTree>(x => x.AvailableChildren)
                .Select(x => x.GetType())
                .Distinct())
            {
                var alreadyRegisteredConditionTypeInfo = AbstractTypeFactory<IConditionTree>.FindTypeInfoByName(conditionType.Name);

                if (alreadyRegisteredConditionTypeInfo == null)
                {
                    AbstractTypeFactory<IConditionTree>.RegisterType(conditionType);
                }
                else
                {
                    // Need to throw exception to prevent registering IConditionTree descendant condition with the same name - or one type deserialization would be broken by another
                    throw new InvalidOperationException($"Cannot register \"{conditionType.Name}\" condition type the one with the same named already registered: \"{alreadyRegisteredConditionTypeInfo.Type.FullName}\"");
                }
            }

            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<AssociationChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<LogChangesChangedEventHandler>().Handle(message));

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<AssociationsModuleDbContext>();
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
            await _appBuilder.ApplicationServices.GetRequiredService<AssociationsExportImport>().DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<AssociationsExportImport>().DoImportAsync(inputStream, progressCallback, cancellationToken);
        }
    }
}
