using DgPersonal.Persistence.Classes;
using DgPersonal.Persistence.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DgPersonal.Persistence
{
    public static class ExtensionMethods
    {
        public static void ConfigurePersistenceObjects<TDbContext>(this IServiceCollection services)
            where TDbContext : IDbContext
        {
            services.AddScoped(typeof(IDbContext), provider => provider.GetService<TDbContext>());
            services.AddTransient<IDapperQueryHandler, DapperQueryHandler>();
            services.AddTransient<IEntityFrameworkModelDeleter, EntityFrameworkModelDeleter>();
            services.AddTransient(typeof(IEntityFrameworkModelEditor<,>), typeof(EntityFrameworkModelEditor<,>));
            services.AddTransient(typeof(IEntityFrameworkDependentModelEditor<,>), typeof(EntityFrameworkDependentModelEditor<,>));
        }
    }
}