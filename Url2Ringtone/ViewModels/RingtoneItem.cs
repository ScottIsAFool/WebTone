using System;
using System.ComponentModel;

namespace Url2Ringtone
{
    public class RingtoneItem : INotifyPropertyChanged
    {
        private string originalUrl = "";
        public string OriginalUrl
        {
            get { return originalUrl; }
            set
            {
                if (value != originalUrl)
                {
                    originalUrl = value;
                    NotifyPropertyChanged("OriginalUrl");
                }
            }
        }

        private string localUrl = "";
        public string LocalUrl
        {
            get { return localUrl; }
            set
            {
                if (value != localUrl)
                {
                    localUrl = value;
                    NotifyPropertyChanged("LocalUrl");
                }
            }
        }

        private bool isStoredLocally;
        public bool IsStoredLocally
        {
            get { return isStoredLocally; }
            set
            {
                isStoredLocally = value;
                NotifyPropertyChanged("IsStoredLocally");
            }
        }

        private string ringtoneName = "";
        public string RingtoneName
        {
            get { return ringtoneName; }
            set
            {
                if (value != ringtoneName)
                {
                    ringtoneName = value;
                    NotifyPropertyChanged("RingtoneName");
                }
            }
        }

        private long toneSize;
        public long ToneSize
        {
            get { return toneSize; }
            set
            {
                if (value != toneSize)
                {
                    toneSize = value;
                    ToneSizeString = "";
                    NotifyPropertyChanged("ToneSize");
                }
            }
        }

        private string toneSizeString;
        public string ToneSizeString
        {
            get { return toneSizeString; }
            set
            {
                string prefix = Url2Ringtone.Resources.Strings.RingtoneSize;
                double finalSize = 0;
                string byteSize = "";
                bool isZero = false;

                if (toneSize == 0)
                {
                    toneSizeString = prefix;
                    isZero = true;
                }
                else if (toneSize < 1024)
                {
                    finalSize = toneSize;
                    byteSize = "BY";
                }
                else if (toneSize < (1024 * 1024))
                {
                    finalSize = toneSize / 1024;
                    byteSize = "KB";
                }
                else if (toneSize < (1024 * 1024 * 1024))
                {
                    finalSize = (toneSize / 1024) / 1024;
                    byteSize = "MB";
                }
                if (!isZero)
                    toneSizeString = string.Format("{0}{1}{2}", prefix, finalSize, byteSize);
                NotifyPropertyChanged("ToneSizeString");
            }
        }

        public string TempFileName { get; set; }

        public string FileExtension { get; set; }

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