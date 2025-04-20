using Space_FaceID.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Space_FaceID.Views.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for UserEditDialogView.xaml
    /// </summary>
    public partial class UserEditDialogView : UserControl
    {
        private readonly UserEditDialogViewModel _viewModel;

        public UserEditDialogView(UserEditDialogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }
    }
}
