using Microsoft.Extensions.Hosting;
using Space_FaceID.Data.Seed;
using Space_FaceID.DI;
using Space_FaceID.Views.Windows;
using System.Windows;

namespace Space_FaceID
{
    public partial class App : Application
    {
        private readonly IHost _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.ConfigureServices();
                })
                .Build();

        public T? GetService<T>() where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            SQLitePCL.Batteries.Init();

            await _host.StartAsync();

            // เพิ่มข้อมูลเริ่มต้นลงในฐานข้อมูล
            await DataSeeder.SeedDatabase(_host);

            MainWindow = GetService<MainWindow>();
            MainWindow?.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }
    }

}
