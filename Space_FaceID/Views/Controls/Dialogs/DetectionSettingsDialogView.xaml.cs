using Space_FaceID.ViewModels;
using Space_FaceID.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Space_FaceID.Views.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for DetectionSettingsDialogView.xaml
    /// </summary>
    public partial class DetectionSettingsDialogView : UserControl
    {
        private readonly DetectionSettingsDialogViewModel _viewModel;

        public DetectionSettingsDialogView(DetectionSettingsDialogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            Loaded += DetectionSettingsDialogView_Loaded;
        }

        private async void DetectionSettingsDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
