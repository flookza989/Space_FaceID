using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Space_FaceID.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Space_FaceID.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DispatcherTimer _timer;

        // เก็บ cache ของ UserControl
        private readonly Dictionary<string, UserControl> _viewCache = [];

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private DateTime _currentDate;

        [ObservableProperty]
        private string _currentTime;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            CurrentDate = DateTime.Now;
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();


            // สร้าง instance Dashboard และเก็บไว้ใน cache
            var dashboardView = _serviceProvider.GetRequiredService<DashboardUserControl>();
            _viewCache["Dashboard"] = dashboardView;
            CurrentView = dashboardView;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentDate = DateTime.Now;
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }

        [RelayCommand]
        private void NavigateTo(string viewName)
        {
            // ตรวจสอบว่ามีใน cache หรือไม่
            if (!_viewCache.TryGetValue(viewName, out var view))
            {
                // ถ้าไม่มีใน cache ให้สร้างใหม่และเก็บไว้
                view = viewName switch
                {
                    "Dashboard" => _serviceProvider.GetRequiredService<DashboardUserControl>(),
                    "Employees" => _serviceProvider.GetRequiredService<EmployeeUserControl>(),
                    "AccessLogs" => _serviceProvider.GetRequiredService<DashboardUserControl>(),
                    _ => _serviceProvider.GetRequiredService<DashboardUserControl>(),
                };

                _viewCache[viewName] = view;
            }

            CurrentView = view;
        }

        public void Cleanup()
        {
            // หยุด timer เมื่อไม่ได้ใช้งานแล้ว
            _timer.Stop();
            _timer.Tick -= Timer_Tick;

            // ทำความสะอาดทรัพยากรทุก view ที่เก็บไว้ใน cache
            foreach (var view in _viewCache.Values)
            {
                // ถ้า view มี ViewModel ที่ implement IDisposable ให้เรียก Dispose
                if (view.DataContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // ล้าง cache
            _viewCache.Clear();
        }
    }
}
