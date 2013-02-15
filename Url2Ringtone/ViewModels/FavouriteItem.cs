using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Url2Ringtone
{
    public class FavouriteItem : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private Uri _url;
        public Uri Url
        {
            get { return _url; }
            set
            {
                if (value != _url)
                {
                    _url = value;
                    NotifyPropertyChanged("Url");
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
