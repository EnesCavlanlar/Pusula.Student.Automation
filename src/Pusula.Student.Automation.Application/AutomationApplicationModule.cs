using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Pusula.Student.Automation.Messaging; // ⬅ bizim publisher için

namespace Pusula.Student.Automation
{
    [DependsOn(
        typeof(AutomationDomainModule),
        typeof(AutomationApplicationContractsModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpFeatureManagementApplicationModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpAccountApplicationModule),
        typeof(AbpTenantManagementApplicationModule),
        typeof(AbpSettingManagementApplicationModule)
        )]
    public class AutomationApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // AutoMapper
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<AutomationApplicationModule>();
            });

            // ⬅ RabbitMQ publisher'ı DI konteyner'e ekle
            context.Services.AddSingleton<IRabbitMqEventPublisher, RabbitMqEventPublisher>();
        }
    }
}
