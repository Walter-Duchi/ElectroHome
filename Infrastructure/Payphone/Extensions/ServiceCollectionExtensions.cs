using Infrastructure.Payphone.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Payphone.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPayphoneServices(this IServiceCollection services)
        {
            services.AddScoped<IPayphoneService, PayphoneService>();
            services.AddHttpClient();
            return services;
        }
    }
}