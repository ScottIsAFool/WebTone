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
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;
using System.IO;
using System.ComponentModel;
using Url2Ringtone.Resources;
using System.Text.RegularExpressions;

namespace Url2Ringtone
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool isPlay = true;
        bool isEdit = false;
        bool resumeMediaPlayerAfterDone;
        bool AlreadyAskedForPermission = false;
        bool MusicPlaying = !MediaPlayer.GameHasControl;

        SaveRingtoneTask saveRingtoneChooser;

        GameTimer timer;
        
        ApplicationBarIconButton selectHistoryItems;
        ApplicationBarIconButton deleteHistoryItems;
        ApplicationBar selectedItemsMenu;
        ApplicationBar normalMenu;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            App.ViewModel.DataLoaded += new EventHandler(ViewModel_DataLoaded);
            App.ViewModel.ErrorHappened += new EventHandler(ViewModel_ErrorHappened);

            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += new EventHandler<GameTimerEventArgs>(timer_Update);

            timer.Start();

            saveRingtoneChooser = new SaveRingtoneTask();
            saveRingtoneChooser.Completed += new EventHandler<TaskEventArgs>(saveRingtoneChooser_Completed);

            CreateApplicationBars();

#if DEBUG
            //Clipboard.SetText("http://fsa.zedge.net/mobile-download-ext/4-1011190-86183967/m4r/Alexander_Meerkat.m4a");
            //Clipboard.SetText("http://forums.syfy.com/index.php?app=core&module=attach&section=attach&attach_id=17659");
            Clipboard.SetText("http://bit.ly/nyancatmp3");
            //Clipboard.SetText("http://bit.ly/nxpSFH");
            //Clipboard.SetText("http://api.ning.com/files/3zmSvhA*3jKxFJj1I5uh5dp5oCynyyMksQjwS3JWWQNlriTzDzX61KtlFnuQtx-hEmV7NdqVgofmZvh7cXOX-UVJ47m1SR4a/nyanlooped.mp3");
#endif
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LittleWatson.CheckForPreviousException();
        }

        void ViewModel_ErrorHappened(object sender, EventArgs e)
        {
            errorMessageBox.ShowMessage((string)sender);
        }

        private void CreateApplicationBars()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = Strings.Play;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = Strings.Save;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = Strings.Browse;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).Text = Strings.Favourites;
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = Strings.Settings;
            normalMenu = (ApplicationBar)ApplicationBar;
            selectedItemsMenu = new ApplicationBar();
            selectHistoryItems = new ApplicationBarIconButton();

            selectHistoryItems.Text = Strings.Select;
            selectHistoryItems.IconUri = new Uri("/Icons/appbar.manage.rest.png", UriKind.Relative);
            selectHistoryItems.Click += new EventHandler(selectHistoryItems_Click);

            deleteHistoryItems = new ApplicationBarIconButton();
            deleteHistoryItems.Text = Strings.Delete;
            deleteHistoryItems.IconUri = new Uri("/Icons/appbar.delete.rest.png", UriKind.Relative);
            deleteHistoryItems.Click += new EventHandler(deleteHistoryItems_Click);
            
            ApplicationBarIconButton cancelDeletion = new ApplicationBarIconButton();
            cancelDeletion.Text = Strings.Cancel;
            cancelDeletion.IconUri = new Uri("/Icons/appbar.close.rest.png", UriKind.Relative);
            cancelDeletion.Click += new EventHandler(cancelDeletion_Click);

            selectedItemsMenu.Buttons.Add(deleteHistoryItems);
            selectedItemsMenu.Buttons.Add(cancelDeletion);
        }

        void selectHistoryItems_Click(object sender, EventArgs e)
        {
            lbHistory.IsSelectionEnabled = true;
            ApplicationBar = selectedItemsMenu;
        }

        void cancelDeletion_Click(object sender, EventArgs e)
        {
            lbHistory.IsSelectionEnabled = false;
            ApplicationBar = normalMenu;
        }

        void deleteHistoryItems_Click(object sender, EventArgs e)
        {
            if (lbHistory.SelectedItems.Count > 0)
            {
                if (ringtonePlayer.CurrentState == MediaElementState.Playing)
                    ringtonePlayer.Stop();
                try
                {
                    foreach (RingtoneItem item in lbHistory.SelectedItems)
                    {
                        App.ViewModel.DeleteItem(item);
                    }
                }
                catch { }
            }
            cancelDeletion_Click(sender, e);
        }

        void ViewModel_DataLoaded(object sender, EventArgs e)
        {
            AskForPermissionToPlay();
            
        }

        private void AskForPermissionToPlay()
        {
            AskForPermissionToPlay(false);
        }

        private void AskForPermissionToPlay(bool isPlayButton)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            if (!App.ViewModel.AutoPlay && !AlreadyAskedForPermission && MusicPlaying)
            {
                if (MessageBox.Show(Strings.PauseMusicText, Strings.PauseMusicTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    StartMusic();
                    AlreadyAskedForPermission = true;
                }
            }
            else if(App.ViewModel.AutoPlay)
            {
                StartMusic();
            }
            else if (isPlayButton)
            {
                StartMusic();
            }
        }

        private void StartMusic()
        {
            SetSource();
            ZunePause();
            ringtonePlayer.Play();
        }

        private void SetSource()
        {
            if (!string.IsNullOrEmpty(App.ViewModel.CurrentItem.LocalUrl))
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(App.ViewModel.CurrentItem.LocalUrl, FileMode.Open, FileAccess.Read))
                    {
                        ringtonePlayer.SetSource(fileStream);
                    }
                }
            }
        }

        private void ZunePause()
        {
            // Please see the MainPage() constructor above where the GameTimer object is created.
            // This enables the use of the XNA framework MediaPlayer class by pumping the XNA FrameworkDispatcher.

            // Pause the Zune player if it is already playing music.
            if (MusicPlaying)
            {
                resumeMediaPlayerAfterDone = true;
                MediaPlayer.Pause();
            }
        }

        private void ZuneResume()
        {
            // If Zune was playing music, resume playback
            if (resumeMediaPlayerAfterDone)
            {
                MediaPlayer.Resume();
            }
        }

        void saveRingtoneChooser_Completed(object sender, TaskEventArgs e)
        {
            if (e.Error != null)
            {
                errorMessageBox.ShowMessage(e.Error.Message);
            }
            else
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    //Check for whether the ringtone needs to be saved locally. Then add to history.
                    if (App.ViewModel.CurrentItem.IsStoredLocally)
                    {
                        App.ViewModel.SaveRingtoneLocally();
                    }
                    else
                    {
                        App.ViewModel.CurrentItem.LocalUrl = string.Empty;
                    }
                    if (!isEdit)
                        App.ViewModel.Items.Add(App.ViewModel.CurrentItem);
                    isEdit = false;
                    App.ViewModel.CurrentItem = new RingtoneItem();
                }
            }
        }

        void timer_Update(object sender, GameTimerEventArgs e)
        {
            FrameworkDispatcher.Update();
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (App.ViewModel.IsDataLoaded)
            {
                if (isPlay)
                {
                    if (ringtonePlayer.AudioStreamCount == 0)
                    {
                        AskForPermissionToPlay(true);                        
                    }
                    else
                    {
                        ringtonePlayer.Play();
                    }
                    if (ringtonePlayer.CurrentState == MediaElementState.Playing)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Icons/appbar.transport.pause.rest.png", UriKind.Relative);
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = Strings.Pause;
                        isPlay = false;
                    }
                }
                else
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Icons/appbar.transport.play.rest.png", UriKind.Relative);
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = Strings.Play;
                    isPlay = true;
                    ringtonePlayer.Pause();
                    ZuneResume();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DoRingtoneSave();
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            DoTheDownload();
        }

        private void DoTheDownload()
        {
            btnGo.Focus();
            if (!App.ViewModel.CurrentItem.OriginalUrl.ToLower().StartsWith("http://") &&
                !App.ViewModel.CurrentItem.OriginalUrl.ToLower().StartsWith("https://") &&
                !App.ViewModel.CurrentItem.OriginalUrl.ToLower().StartsWith("ftp://"))
            {
                Match match = Regex.Match(App.ViewModel.CurrentItem.OriginalUrl, @"([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)");
                if (match.Success)
                {
                    App.ViewModel.CurrentItem.OriginalUrl = string.Format("http://{0}", App.ViewModel.CurrentItem.OriginalUrl);
                }
                else
                {
                    var root = Application.Current.RootVisual as PhoneApplicationFrame;
                    string bingSearchUrl = string.Format("http://www.bing.com/search?q={0}", HttpUtility.UrlEncode(App.ViewModel.CurrentItem.OriginalUrl));
                    App.BrowserModel.NavigateUrl = new Uri(bingSearchUrl, UriKind.Absolute);
                    root.Navigate(new Uri("/Views/Browser.xaml", UriKind.Relative));
                    return;
                }
            }
            ringtonePlayer.Stop();
            App.ViewModel.DownloadMP3();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            ringtonePlayer.Source = null;
        }

        private void ringtonePlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (MediaElementState.Playing == ringtonePlayer.CurrentState)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Icons/appbar.transport.pause.rest.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = Strings.Pause;
                isPlay = false;
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Icons/appbar.transport.play.rest.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = Strings.Play;
                isPlay = true;               
            }
        }

        private void txbxRingtoneName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = App.ViewModel.CurrentItem.RingtoneName.Length > 0;
        }

        private void txbxRingtoneUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoTheDownload();
            }
        }

        private void btnAddRingtone_Click(object sender, RoutedEventArgs e)
        {
            DoRingtoneSave();
        }

        private void DoRingtoneSave()
        {            
            saveRingtoneChooser.DisplayName = App.ViewModel.CurrentItem.RingtoneName;
            saveRingtoneChooser.IsShareable = true;
            saveRingtoneChooser.Source = new Uri(@"isostore:/" + App.ViewModel.CurrentItem.LocalUrl);

            if (ringtonePlayer.CurrentState == MediaElementState.Playing)
                ringtonePlayer.Stop();

            saveRingtoneChooser.Show();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (App.ViewModel.IsDataLoaded && App.ViewModel.CurrentItem.LocalUrl == "temp.mp3")
            {
                AskForPermissionToPlay();
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            }
            string url = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("url", out url))
            {
                if (App.ViewModel.CurrentItem.OriginalUrl.ToLower() !=
                    url.ToLower())
                {
                    App.ViewModel.CurrentItem.OriginalUrl = HttpUtility.UrlDecode(url);
                    DoTheDownload();
                }
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (lbHistory.IsSelectionEnabled)
            {
                lbHistory.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        private void lbHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationBar = lbHistory.IsSelectionEnabled ? selectedItemsMenu : normalMenu;
        }

        private void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            var sendEmail = new EmailComposeTask { To = "scottisafool@live.co.uk", Subject = Strings.FeedbackSubject };
            sendEmail.Show();
        }

        private void btnTwitter_Click(object sender, RoutedEventArgs e)
        {
            var webBrowser = new WebBrowserTask { Uri = new Uri("http://twitter.com/scottisafool", UriKind.Absolute) };
            webBrowser.Show();
        }

        private void btnWebsite_Click(object sender, RoutedEventArgs e)
        {
            var webBrowser = new WebBrowserTask { Uri = new Uri("http://dev.scottisafool.co.uk", UriKind.Absolute) };
            webBrowser.Show();
        }

        private void btnRate_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask mdTask = new MarketplaceDetailTask();
            mdTask.Show();
        }

        private void menuSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Settings.xaml", UriKind.Relative));
        }

        private void linkMike_Click(object sender, RoutedEventArgs e)
        {
            var webBrowser = new WebBrowserTask { Uri = new Uri("http://mikehilton.wordpress.com", UriKind.Absolute) };
            webBrowser.Show();
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RingtoneItem item = ((FrameworkElement)sender).DataContext as RingtoneItem;
            if (lbHistory.IsSelectionEnabled)
            {
                MultiselectItem container = lbHistory.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
                if (container != null)
                {
                    container.IsSelected = !container.IsSelected;
                }
            }
            else
            {
                App.ViewModel.CurrentItem = item;
                App.ViewModel.IsDataLoaded = !string.IsNullOrEmpty(App.ViewModel.CurrentItem.LocalUrl);
                App.ViewModel.DownloadingMP3 = !App.ViewModel.CurrentItem.IsStoredLocally;
                isEdit = true;
                AskForPermissionToPlay();
            }
        }

        private void linkTomer_Click(object sender, RoutedEventArgs e)
        {
            var webBrowser = new WebBrowserTask { Uri = new Uri("http://blogs.microsoft.co.il/blogs/tomershamam/archive/2010/10/19/windows-phone-7-custom-message-box.aspx", UriKind.Absolute) };
            webBrowser.Show();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Browser.xaml", UriKind.Relative));
        }

        private void btnFavourties_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Favourites.xaml", UriKind.Relative));
        }

        private void txbxRingtoneUrl_ActionIconTapped(object sender, EventArgs e)
        {
            App.ViewModel.CurrentItem.OriginalUrl = string.Empty;
        }

        private void linkBlog_Click(object sender, RoutedEventArgs e)
        {
            var webBrowser = new WebBrowserTask { Uri = new Uri("http://webtoneapp.wordpress.com/", UriKind.Absolute) };
            webBrowser.Show();
        }        
    }
}