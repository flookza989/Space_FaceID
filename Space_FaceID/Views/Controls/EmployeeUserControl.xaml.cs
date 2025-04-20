using Space_FaceID.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Space_FaceID.Views.Controls
{
    /// <summary>
    /// Interaction logic for EmployeeUserControl.xaml
    /// </summary>
    public partial class EmployeeUserControl : UserControl
    {
        private readonly EmployeeViewModel _viewModel;

        public EmployeeUserControl(EmployeeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            Loaded += EmployeeUserControl_Loaded;
            Unloaded += EmployeeUserControl_Unloaded;
        }

        private async void EmployeeUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // โหลดข้อมูลเฉพาะครั้งแรกเท่านั้น ถ้ามีข้อมูลแล้วไม่ต้องโหลดใหม่
            if (_viewModel.Employees.Count == 0)
            {
                await _viewModel.InitializeAsync();
            }
        }

        private void EmployeeUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // ไม่ต้องเรียก Dispose ตอนเปลี่ยนหน้า เพราะจะเรียกตอนปิดโปรแกรมแทน
            // _viewModel.Dispose();
        }
    }
}
