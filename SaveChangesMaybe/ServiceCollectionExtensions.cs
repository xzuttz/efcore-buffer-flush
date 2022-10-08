using Microsoft.Extensions.DependencyInjection;

namespace SaveChangesMaybe
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSaveChangesMaybeServiceFactory(this IServiceCollection services)
        {
            services.AddSingleton<ISaveChangesMaybeServiceFactory, SaveChangesMaybeServiceFactory>();
        }
    }
}
