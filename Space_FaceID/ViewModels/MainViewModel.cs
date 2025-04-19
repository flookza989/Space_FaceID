﻿using CommunityToolkit.Mvvm.ComponentModel;
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

namespace Space_FaceID.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DispatcherTimer _timer;

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


            CurrentView = _serviceProvider.GetRequiredService<DashboardUserControl>();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentDate = DateTime.Now;
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }

        [RelayCommand]
        private void NavigateTo(string viewName)
        {
            CurrentView = viewName switch
            {
                "Dashboard" => _serviceProvider.GetRequiredService<DashboardUserControl>(),
                "Employees" => _serviceProvider.GetRequiredService<DashboardUserControl>(),
                "AccessLogs" => _serviceProvider.GetRequiredService<DashboardUserControl>(),
                _ => _serviceProvider.GetRequiredService<DashboardUserControl>(),
            };
        }

        public void Cleanup()
        {
            // หยุด timer เมื่อไม่ได้ใช้งานแล้ว
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }
}
