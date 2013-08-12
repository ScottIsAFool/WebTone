using System;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Net;
using System.Windows.Controls;
using Url2Ringtone.ViewModel;
using System.Windows;
using Url2Ringtone.Resources;

namespace Url2Ringtone
{
    public partial class Browser : PhoneApplicationPage
    {
        private bool HasCheckedForContentType;

        public Browser()
        {
            InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string url = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("InitialUrl", out url))
            {
                fullBrowser.InitialUri = HttpUtility.UrlDecode(url);
            }
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (box.Text.Length > 0)
                {
                    fullBrowser.Navigate(box.Text);
                    this.Focus();
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            fullBrowser.NavigateBack();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            fullBrowser.RefreshBrowser();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            fullBrowser.NavigateForward();
        }

        private void btnAddFavourite_Click(object sender, EventArgs e)
        {
            var bVM = ((ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;
            FavouriteItem newItem = new FavouriteItem { Title = fullBrowser.PageSource.GetTitle(), Url = new Uri(fullBrowser.CurrentUrl) };
            bVM.Favourites.Add(newItem);
            NavigationService.Navigate(new Uri(string.Format("/Views/AddEditFavourite.xaml?action=edit&index={0}", bVM.Favourites.Count - 1), UriKind.Relative));
        }       

        private void fullBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            bool proceedWithChecks = false;
            string extn = System.IO.Path.GetExtension(e.Uri.ToString());
            if (extn.ToLower().StartsWith(".mp3") || extn.ToLower().StartsWith(".wma"))
            {
                AskToDownloadFile(e.Uri);
                return;
            }
            if (App.BrowserModel.UseSmartBrowsing)
                proceedWithChecks = CheckUrlString(e.Uri);
            if (proceedWithChecks || (!App.BrowserModel.UseSmartBrowsing && !App.BrowserModel.DontCheckLinks))
            {
                //e.Cancel = !HasCheckedForContentType;
                //if (!HasCheckedForContentType)
                    CheckForContentType(e.Uri);
                //if (HasCheckedForContentType) HasCheckedForContentType = false;
            }
        }

        private bool CheckUrlString(Uri uri)
        {
            string url = uri.ToString();
            return !(url.ToLower().StartsWith("https") &&
                    (url.ToLower().Contains("auth") ||
                    url.ToLower().Contains("login")));
        }

        private void fullBrowser_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private async Task CheckForContentType(Uri uri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            if (App.ViewModel.Cookies != null)
            {
                try
                {
                    req.CookieContainer = new CookieContainer();
                    req.CookieContainer.Add(uri, App.ViewModel.Cookies);
                }
                catch (Exception ex) { }
            }
            var task = req.GetResponseAsync();
            try
            {
                HttpWebResponse res = (HttpWebResponse) await task.ConfigureAwait(false);
                if (res.ContentType.StartsWith("audio"))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            fullBrowser.ShowProgress = false;
                            AskToDownloadFile(res.ResponseUri);
                        });
                }
                //else
                //{
                //    Deployment.Current.Dispatcher.BeginInvoke(() =>
                //    {
                //        fullBrowser.Navigate(uri.ToString());
                //    });
                //    HasCheckedForContentType = true;
                //}
            }
            catch (Exception ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        fullBrowser.ShowProgress = false;
                    });
            }
        }

        private void AskToDownloadFile(Uri uri)
        {
            var messageBox = new CustomMessageBox
            {
                Title = Strings.FileFoundTitle,
                Message = Strings.FileFoundText,
                LeftButtonContent = Strings.Yes,
                RightButtonContent = Strings.No
            };

            messageBox.Dismissed += (sender, args) =>
            {
                if (args.Result == CustomMessageBoxResult.LeftButton)
                {
                    App.BrowserModel.AudioFileFound(uri, fullBrowser.GetCookies());
                }
            };
            messageBox.Show();
        }

        private void btnSmartBrowsing_Click(object sender, EventArgs e)
        {
            App.BrowserModel.DontCheckLinks = !App.BrowserModel.DontCheckLinks;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Settings.xaml", UriKind.Relative));
        }

        private void btnRecent_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/History.xaml", UriKind.Relative));
        }

        private void PhoneTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            fullBrowser.CurrentUrl = string.Empty;
        }
    }
}