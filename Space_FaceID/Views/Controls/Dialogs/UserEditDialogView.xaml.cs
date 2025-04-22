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
        public UserEditDialogView(UserEditDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
