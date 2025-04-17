using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Space_FaceID.Data.Seed;
using Space_FaceID.DI;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Space_FaceID
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.ConfigureServices();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            SQLitePCL.Batteries.Init();


            await _host.StartAsync();

            // เพิ่มข้อมูลเริ่มต้นลงในฐานข้อมูล
            await DataSeeder.SeedDatabase(_host);

            var mainWindow = new MainWindow
            {
                //DataContext = _host.Services.GetRequiredService<MainViewModel>()
            };

            mainWindow.Show();

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
