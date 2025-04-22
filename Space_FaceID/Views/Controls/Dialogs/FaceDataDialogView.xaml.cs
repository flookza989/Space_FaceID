using Space_FaceID.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Space_FaceID.Views.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for FaceDataDialogView.xaml
    /// </summary>
    public partial class FaceDataDialogView : UserControl
    {
        private readonly FaceDataDialogViewModel _viewModel;

        public FaceDataDialogView(FaceDataDialogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            Loaded += FaceDataDialogView_Loaded;
            Unloaded += FaceDataDialogView_Unloaded;
        }

        private void FaceDataDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            // ไม่ต้องทำอะไรเพิ่มเติม เนื่องจาก ViewModel จะโหลดกล้องเอง
        }

        private void FaceDataDialogView_Unloaded(object sender, RoutedEventArgs e)
        {
            // หยุดกล้องเมื่อปิดไดอะล็อก
            _viewModel.Dispose();
        }
    }
}
