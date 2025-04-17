using Microsoft.Extensions.DependencyInjection;

namespace Space_FaceID.DI
{
    public static class ContainerConfig
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            var assembly = typeof(App).Assembly;

            // Auto register Services and Repositories
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            // Auto register ViewModels
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("ViewModel") && !type.IsAbstract))
                    .AsSelf()
                    .WithTransientLifetime());

            // เพิ่มการลงทะเบียน DbContext ถ้าใช้ EF Core
            // services.AddDbContext<AppDbContext>(options => 
            //     options.UseSqlite("Data Source=facerecognition.db"));

            return services;
        }
    }
}
