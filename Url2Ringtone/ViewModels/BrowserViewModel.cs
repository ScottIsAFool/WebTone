using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using System.Collections;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Linq;
using ScottIsAFool.WindowsPhone.IsolatedStorage;
using Url2Ringtone.Resources;

namespace Url2Ringtone
{
    public class BrowserViewModel : INotifyPropertyChanged
    {
        public BrowserViewModel()
        {
            Favourites = new ObservableCollection<FavouriteItem>();
            History = new ObservableCollection<HistoryItem>();
            SelectionChangedCommand = new RelayCommand<IList>(items =>
            {
                if (items == null)
                {
                    NumberOfItemsSelected = 0;
                    return;
                }

                NumberOfItemsSelected = items.Count;
                if (items.Count == 1)
                {
                    ItemToEdit = items[0] as FavouriteItem;
                }
            });

            ItemTappedCommand = new RelayCommand<Uri>(fave =>
            {
                NavigateUrl = fave;
                var root = Application.Current.RootVisual as PhoneApplicationFrame;
                root.Navigate(new Uri("/Views/Browser.xaml", UriKind.Relative));
            });

            DeleteItemsCommand = new RelayCommand<IList>(items =>
            {
                if (items.Count > 0)
                {
                    if (MessageBox.Show(Strings.DeleteFavouritesText, Strings.ClearLocalFilesTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        var bVM = ((Url2Ringtone.ViewModel.ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;
                        for (int i = 0; i < items.Count; i++)
                        {
                            bVM.Favourites.Remove((FavouriteItem)items[i]);
                        }
                    }
                }
            });

            AddFavouriteCommand = new RelayCommand(() =>
            {
                var root = Application.Current.RootVisual as PhoneApplicationFrame;
                root.Navigate(new Uri("/Views/AddEditFavourite.xaml?action=add", UriKind.Relative));
            });

            HistoryItemTappedCommand = new RelayCommand<Uri>(historyItem =>
            {
                var root = Application.Current.RootVisual as PhoneApplicationFrame;
                NavigateUrl = historyItem;
                root.GoBack();
            });

            HistoryDeleteCommand = new RelayCommand(() =>
            {
                if (MessageBox.Show(Strings.DeleteHistoryText, Strings.ClearLocalFilesTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    History.Clear();
                }
            });

            SmartBrowsingText = DontCheckLinks ? Strings.DontCheckLinks : Strings.CheckLinks;
        }

        public ObservableCollection<FavouriteItem> Favourites { get; internal set; }
        public ObservableCollection<HistoryItem> History { get; set; }

        private bool _pageIsLoading;
        public bool PageIsLoading
        {
            get { return _pageIsLoading; }
            set
            {
                _pageIsLoading = value;
                NotifyPropertyChanged("PageIsLoading");
            }
        }

        private string _currentBrowsingUrl;
        public string CurrentBrowsingUrl
        {
            get { return _currentBrowsingUrl; }
            set
            {
                if (value != _currentBrowsingUrl)
                {
                    _currentBrowsingUrl = value;
                    NotifyPropertyChanged("CurrentBrowsingUrl");
                }
            }
        }

        public RelayCommand<IList> SelectionChangedCommand { get; private set; }
        public RelayCommand<Uri> ItemTappedCommand { get; private set; }
        public RelayCommand<IList> DeleteItemsCommand { get; private set; }
        public RelayCommand AddFavouriteCommand { get; private set; }
        public RelayCommand<Uri> HistoryItemTappedCommand { get; private set; }
        public RelayCommand HistoryDeleteCommand { get; private set; }

        public void AudioFileFound(Uri downloadUrl, CookieCollection cookies)
        {
            App.ViewModel.Cookies = cookies;
            var root = Application.Current.RootVisual as PhoneApplicationFrame;
            root.Navigate(new Uri(string.Format("/Views/MainPage.xaml?url={0}", HttpUtility.UrlEncode(downloadUrl.ToString())), UriKind.Relative));
        }

        private int _numberOfItemsSelected;
        public int NumberOfItemsSelected
        {
            get { return _numberOfItemsSelected; }
            set
            {
                if (value != _numberOfItemsSelected)
                {
                    _numberOfItemsSelected = value;
                    NotifyPropertyChanged("NumberOfItemsSelected");
                }
            }
        }

        public bool UseSmartBrowsing
        {
            get { return ISettings.GetBoolean("SmartBrowsing", true); }
            set
            {
                ISettings.Set("SmartBrowsing", value);                
                NotifyPropertyChanged("UseSmartBrowsing");
            }
        }

        public bool DontCheckLinks
        {
            get { return ISettings.GetBoolean("DontCheckLinks", true); }
            set
            {
                ISettings.Set("DontCheckLinks", value);
                SmartBrowsingText = value ? Strings.DontCheckLinks : Strings.CheckLinks;
                NotifyPropertyChanged("DontCheckLinks");
            }
        }

        private string _smartBrowsingText;
        public string SmartBrowsingText
        {
            get { return _smartBrowsingText; }
            set
            {
                if (value != _smartBrowsingText)
                {
                    _smartBrowsingText = value;
                    NotifyPropertyChanged("SmartBrowsingText");
                }
            }
        }

        private Uri _navigateUrl;
        public Uri NavigateUrl
        {
            get { return _navigateUrl; }
            set
            {
                if (value != _navigateUrl)
                {
                    _navigateUrl = value;
                    NotifyPropertyChanged("NavigateUrl");
                }
            }
        }

        private FavouriteItem _itemToEdit;
        public FavouriteItem ItemToEdit
        {
            get { return _itemToEdit; }
            set
            {
                if (value != _itemToEdit)
                {
                    _itemToEdit = value;
                    NotifyPropertyChanged("ItemToEdit");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
