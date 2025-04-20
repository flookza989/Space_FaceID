using Microsoft.Extensions.DependencyInjection;
using Space_FaceID.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_FaceID.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            MenuListBox.SelectionChanged += MenuListBox_SelectionChanged;
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem == null) return;

            // ตรวจสอบว่าเลือกรายการไหนแล้วสั่งเปลี่ยนหน้า
            var selectedItem = MenuListBox.SelectedItem as ListBoxItem;

            if (selectedItem == DashboardItem)
            {
                _viewModel.NavigateToCommand.Execute("Dashboard");
            }
            else if (selectedItem == EmployeesItem)
            {
                _viewModel.NavigateToCommand.Execute("Employees");
            }
            else if (selectedItem == AccessListItem)
            {
                _viewModel.NavigateToCommand.Execute("AccessLogs");
            }
            else if (selectedItem == DevicesItem || selectedItem == ReportsItem || selectedItem == SettingsItem)
            {
                // ยังไม่มีหน้านี้ ให้กลับไปหน้า Dashboard
                _viewModel.NavigateToCommand.Execute("Dashboard");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // ยกเลิกการลงทะเบียน event handler
            MenuListBox.SelectionChanged -= MenuListBox_SelectionChanged;
            
            // เรียกใช้ Cleanup ของ MainViewModel เพื่อทำความสะอาดทรัพยากรทั้งหมด
            _viewModel.Cleanup(); 
        }
    }
}
