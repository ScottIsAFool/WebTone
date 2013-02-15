using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using ScottIsAFool.WindowsPhone.IsolatedStorage;
using Url2Ringtone.Resources;
using Tomers.Phone.Controls;
using System.Text.RegularExpressions;

namespace Url2Ringtone
{
    public class MainViewModel : INotifyPropertyChanged
    {
        WebClient downloadMP3Client;
        private Uri FinalUrl;
        IsolatedStorageFileStream isolatedStorageFileStream;
        IsolatedStorageFile isolatedStorageFile;

        public MainViewModel()
        {
            Items = new ObservableCollection<RingtoneItem>();
            CurrentItem = new RingtoneItem();
            CurrentItem.ToneSizeString = "";
            try
            {
                isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
            }
            catch { }
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<RingtoneItem> Items { get; set; }

        private bool isDataLoaded = true;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set
            {
                isDataLoaded = value;
                NotifyPropertyChanged("IsDataLoaded");
            }
        }

        private bool downloadingMP3 = true;
        public bool DownloadingMP3
        {
            get { return downloadingMP3; }
            set
            {
                downloadingMP3 = value;
                NotifyPropertyChanged("DownloadingMP3");
            }
        }

        private RingtoneItem currentItem;
        public RingtoneItem CurrentItem
        {
            get { return currentItem; }
            set
            {
                currentItem = value;
                NotifyPropertyChanged("CurrentItem");
            }
        }

        private double progressChanged;
        public double ProgressChanged
        {
            get { return progressChanged; }
            set
            {
                if (value != progressChanged)
                {
                    progressChanged = value;
                    NotifyPropertyChanged("PercentageChanged");
                }
            }
        }

        private bool isIndeterminate;
        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                isIndeterminate = value;
                NotifyPropertyChanged("IsIndeterminate");
            }
        }


        private string progressText;
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                if (value != progressText)
                {
                    progressText = value;
                    NotifyPropertyChanged("ProgressText");
                }
            }
        }

        public bool AutoPlay
        {
            get { return ISettings.GetBoolean("AutoPlay", false); }
            set
            {
                ISettings.Set("AutoPlay", value);
                NotifyPropertyChanged("AutoPlay");
            }
        }

        public bool IsNetworkEnabled
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }

        public bool IsDarkTheme
        {
            get
            {
                return (Application.Current.Resources.Contains("PhoneDarkThemeVisibility") && ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"]) == Visibility.Visible);
            }
        }

        internal CookieCollection Cookies { get; set; }

        public void DownloadMP3()
        {
            if (IsNetworkEnabled)
            {
                if (CurrentItem.OriginalUrl.Length > 0)
                {
                    string fileName = System.IO.Path.GetFileName(CurrentItem.OriginalUrl);
                    DownloadingMP3 = true;
                    IsIndeterminate = true;
                    IsDataLoaded = false;
                    ProgressText = Strings.CheckingLink;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CurrentItem.OriginalUrl);
                    if (Cookies != null)
                    {
                        try
                        {
                            request.CookieContainer = new CookieContainer();
                            request.CookieContainer.Add(new Uri(CurrentItem.OriginalUrl), Cookies);
                        }
                        catch (Exception ex) { }
                    }
                    request.BeginGetResponse(new AsyncCallback(GetData), request);
                }
            }
            else
            {
                SetError(Strings.NoNetworkConnectionError, Strings.Error);
            }
        }

        private void GetData(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    FinalUrl = response.ResponseUri;
                    string extn = System.IO.Path.GetExtension(FinalUrl.LocalPath);
                    if (extn.ToLower().Equals(".mp3") || extn.ToLower().Equals(".wma"))
                    {
                        CurrentItem.FileExtension = extn;
                        DoDownload();
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            NotificationBox.Show(Strings.FileTypeTitle, Strings.FileTypeText,
                                                 new NotificationBoxCommand("wma", () =>
                                                 {
                                                     CurrentItem.FileExtension = ".wma";
                                                     DoDownload();
                                                 }),
                                                 new NotificationBoxCommand("mp3", () =>
                                                 {
                                                     CurrentItem.FileExtension = ".mp3";
                                                     DoDownload();
                                                 }),
                                                 new NotificationBoxCommand(Strings.Cancel, () =>
                                                 {
                                                     DownloadingMP3 = false;
                                                     IsDataLoaded = true;
                                                     IsIndeterminate = false;
                                                 }));
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    DownloadingMP3 = false;
                    IsDataLoaded = true;
                    IsIndeterminate = false;
                    SetError(ex.Message, Strings.Error); 
                });
            }
        }

        private void DoDownload()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressText = Strings.Downloading;
                IsIndeterminate = true;
                ProgressChanged = 0;
            });
            downloadMP3Client = new WebClient();
            
            downloadMP3Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadMP3Client_DownloadProgressChanged);
            downloadMP3Client.OpenReadCompleted += new OpenReadCompletedEventHandler(downloadMP3Client_OpenReadCompleted);
            downloadMP3Client.OpenReadAsync(FinalUrl);
        }

        void downloadMP3Client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (!e.Cancelled)
                    {
                        if (e.Error == null)
                        {
                            try
                            {
                                if (e.Result != null)
                                {
                                    CurrentItem.TempFileName = string.Format("temp{0}", CurrentItem.FileExtension);

                                    CurrentItem.RingtoneName = System.IO.Path.GetFileNameWithoutExtension(FinalUrl.ToString());

                                    if (isolatedStorageFile.FileExists(CurrentItem.TempFileName))
                                    {
                                        isolatedStorageFile.DeleteFile(CurrentItem.TempFileName);
                                    }
                                    bool checkQuotaIncrease = IncreaseIsolatedStorageSpace(e.Result.Length);

                                    string AudioFile = CurrentItem.TempFileName;
                                    using (isolatedStorageFileStream = new IsolatedStorageFileStream(AudioFile, FileMode.Create, isolatedStorageFile))
                                    {
                                        using (BinaryWriter writer = new BinaryWriter(isolatedStorageFileStream))
                                        {
                                            long AudioFileLength = (long)e.Result.Length;
                                            byte[] buffer = new byte[32];
                                            int readCount = 0;
                                            using (BinaryReader reader = new BinaryReader(e.Result))
                                            {
                                                while (readCount < AudioFileLength)
                                                {
                                                    int actual = reader.Read(buffer, 0, buffer.Length);
                                                    readCount += actual;
                                                    writer.Write(buffer, 0, actual);
                                                }
                                            }
                                        }
                                    }
                                    CurrentItem.LocalUrl = AudioFile;
                                    IsDataLoaded = true;
                                    DownloadingMP3 = false;
                                    ProgressText = "";
                                    IsIndeterminate = false;
                                    if (DataLoaded != null)
                                        DataLoaded(this, new EventArgs());
                                }
                            }
                            catch (Exception ex)
                            {
                                SetError(ex.Message, Strings.Error);
                            }
                        }
                        else
                        {
                            SetError(e.Error.Message, Strings.Error);
                        }
                    }
                });
        }

        void downloadMP3Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive > 1048576)
            {
                downloadMP3Client.CancelAsync();
                Deployment.Current.Dispatcher.BeginInvoke(() => { SetError(Strings.FileSizeError, Strings.Information); });
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ProgressChanged = (double)e.ProgressPercentage / 100;
                    CurrentItem.ToneSize = e.TotalBytesToReceive;
                });
            }
        }

        protected bool IncreaseIsolatedStorageSpace(long quotaSizeDemand)
        {
            bool CanSizeIncrease = false;
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
            //Get the Available space
            long maxAvailableSpace = isolatedStorageFile.AvailableFreeSpace;
            if (quotaSizeDemand > maxAvailableSpace)
            {
                try
                {
                    if (!isolatedStorageFile.IncreaseQuotaTo(isolatedStorageFile.Quota + quotaSizeDemand))
                    {
                        CanSizeIncrease = false;
                        return CanSizeIncrease;
                    }
                }
                catch (Exception ex)
                {
                    SetError(ex.Message, Strings.Error);
                }
                CanSizeIncrease = true;
                return CanSizeIncrease;
            }
            return CanSizeIncrease;
        }

        public void SaveRingtoneLocally()
        {
            try
            {
                string date = string.Format("{0:d-M-yyyy-HH-mm-ss}", DateTime.Now);
                string newFile = string.Format("{0}-{1}{2}", date, CurrentItem.RingtoneName, CurrentItem.FileExtension);
                isolatedStorageFile.MoveFile(CurrentItem.TempFileName, newFile);
                CurrentItem.LocalUrl = newFile;
            }
            catch (Exception ex)
            {
                SetError(ex.Message, Strings.Error);
            }
        }

        private void SetError(string message, string title)
        {
            if (ErrorHappened != null)
                ErrorHappened(message, new EventArgs());
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

        public event EventHandler DataLoaded;
        public event EventHandler ErrorHappened;

        internal void DeleteItem(RingtoneItem item)
        {
            if (item.IsStoredLocally)
            {
                if (isolatedStorageFile.FileExists(item.LocalUrl))
                {
                    try
                    {
                        isolatedStorageFile.DeleteFile(item.LocalUrl);
                        item.IsStoredLocally = false;
                    }
                    catch (Exception ex)
                    {
                        SetError(ex.Message, Strings.Error);
                    }
                }
            }
            Items.Remove(item);
        }

        internal void ClearLocalFiles()
        {
            foreach (var item in Items)
            {
                if (item.IsStoredLocally)
                {
                    try
                    {
                        if (isolatedStorageFile.FileExists(item.LocalUrl))
                            isolatedStorageFile.DeleteFile(item.LocalUrl);
                        item.IsStoredLocally = false;
                    }
                    catch (Exception ex)
                    {
                        SetError(ex.Message, Strings.Error);
                    }
                }
            }
            isolatedStorageFile.Remove();
        }

    }
}