using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Url2Ringtone.Resources;

namespace Url2Ringtone
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            
            if (MessageBox.Show(Strings.ClearLocalFilesText, Strings.ClearLocalFilesTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                App.ViewModel.ClearLocalFiles();
            }
        }
    }
}