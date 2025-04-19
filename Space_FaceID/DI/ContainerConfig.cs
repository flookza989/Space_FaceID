using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Space_FaceID.Data.Context;

namespace Space_FaceID.DI
{
    public static class ContainerConfig
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            var assembly = typeof(App).Assembly;

            // ลงทะเบียน DbContext
            services.AddDbContextFactory<FaceIDDbContext>(options =>
                options.UseSqlite("Data Source=faceID.db"));

            // Auto register Services and Repositories
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            // Auto register ViewModels
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("ViewModel") && !type.IsAbstract))
                    .AsSelf()
                    .WithTransientLifetime());

            // Auto register Windows
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Window") && !type.IsAbstract))
                    .AsSelf()
                    .WithTransientLifetime());

            // Auto register UserControls
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("UserControl") && !type.IsAbstract))
                    .AsSelf()
                    .WithScopedLifetime());

            return services;
        }
    }
}
