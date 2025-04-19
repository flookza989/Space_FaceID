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
    /// Interaction logic for DetectionSettingsWindow.xaml
    /// </summary>
    public partial class DetectionSettingsWindow : Window
    {
        private readonly DetectionSettingsViewModel _viewModel;
        public DetectionSettingsWindow(DetectionSettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            _viewModel.RequestClose += () => Close();
            Loaded += DetectionSettingsWindow_Loaded;
            Unloaded += DetectionSettingsWindow_Unloaded;
        }

        private async void DetectionSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }

        private void DetectionSettingsWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.RequestClose -= () => this.Close();
            _viewModel.Dispose();
        }
    }
}
