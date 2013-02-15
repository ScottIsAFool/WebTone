using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using Url2Ringtone.Resources;

namespace Url2Ringtone.Views
{
    public partial class AddEditFavourite : PhoneApplicationPage
    {
        public AddEditFavourite()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string isAdd = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("action", out isAdd))
            {
                if (isAdd.Equals("add"))
                {
                    App.BrowserModel.Favourites.Add(new FavouriteItem());
                    DataContext = App.BrowserModel.Favourites[App.BrowserModel.Favourites.Count - 1];
                }
                else
                {
                    DataContext = App.BrowserModel.ItemToEdit;
                }
                PageTitle.Text = isAdd.Equals("add") ? Strings.AddFavourite : Strings.EditFavourite;
            }
        }

        private void PhoneTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            txbxUrl.Text = string.Empty;
        }
    }
}