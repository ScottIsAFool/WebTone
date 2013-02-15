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

namespace Url2Ringtone
{
    public partial class Favourites : PhoneApplicationPage
    {
        public Favourites()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/AddEditFavourite.xaml?action=add", UriKind.Relative));
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lbFavourites.SelectedItems.Count == 1)
            {                
                NavigationService.Navigate(new Uri("/Views/AddEditFavourite.xaml?action=edit", UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (lbFavourites.IsSelectionEnabled)
            {
                lbFavourites.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }
    }
}