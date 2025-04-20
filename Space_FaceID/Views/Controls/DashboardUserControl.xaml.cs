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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Space_FaceID.Views.Controls
{
    /// <summary>
    /// Interaction logic for DashboardUserControl.xaml
    /// </summary>
    public partial class DashboardUserControl : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardUserControl(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            Loaded += DashboardUserControl_Loaded;
            Unloaded += DashboardUserControl_Unloaded;
        }

        private async void DashboardUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // สำหรับกรณีที่มีการเปลี่ยนหน้ากลับมาที่ Dashboard อีกครั้ง
            if (!_viewModel.IsCameraActive)
            {
                await _viewModel.StartCameraAsync();
            }
        }

        private void DashboardUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopCamera();
        }
    }
}
