///This code is offered as is without any warranties.
///For any information or comments pleas contact:
///Francisco Fernandez
///fj.fernandez@live.com
///Developer

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
using System.Collections.ObjectModel;
using System.Reflection;
using Url2Ringtone;

namespace FullBrowserControl
{
    public partial class FullWebBrowser : UserControl
    {
        //ctr
        public FullWebBrowser()
        {
            InitializeComponent();
        }

        #region Fields

        //The navigation urls of the browser.
        private readonly Stack<Uri> _NavigatingUrls = new Stack<Uri>();

        //The history for the browser
        private readonly ObservableCollection<HistoryItem> _History = 
            new ObservableCollection<HistoryItem>();

        //Flag to check if the browser is navigating back.
        bool _IsNavigatingBackward = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the History property for the browser.
        /// </summary>
        public static readonly DependencyProperty HistoryProperty = 
            DependencyProperty.Register("History", typeof(ObservableCollection<HistoryItem>),
            typeof(FullWebBrowser), new PropertyMetadata(new ObservableCollection<HistoryItem>()));

        public ObservableCollection<HistoryItem> History
        {
            get { return (ObservableCollection<HistoryItem>)GetValue(HistoryProperty); }
            set { SetValue(HistoryProperty, value); }
        }                                   

        public string PageSource
        {
            get { return TheWebBrowser.SaveToString(); }
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// ShowProgress Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShowProgressProperty =
            DependencyProperty.Register("ShowProgress", typeof(bool), 
            typeof(FullWebBrowser), new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the ShowProgress property. This dependency property 
        /// indicates whether to show the progress bar.
        /// </summary>
        public bool ShowProgress
        {
            get { return (bool)GetValue(ShowProgressProperty); }
            set { SetValue(ShowProgressProperty, value); }
        }

        /// <summary>
        /// CanNavigateBack Dependency Property
        /// </summary>
        public static readonly DependencyProperty CanNavigateBackProperty =
            DependencyProperty.Register("CanNavigateBack", typeof(bool), 
            typeof(FullWebBrowser), new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the CanNavigateBack property. This dependency property 
        /// indicates whether the browser can go back.
        /// </summary>
        public bool CanNavigateBack
        {
            get { return (bool)GetValue(CanNavigateBackProperty); }
            set { SetValue(CanNavigateBackProperty, value); }
        }

        /// <summary>
        /// InitialUri Dependency Property
        /// </summary>
        public static readonly DependencyProperty InitialUriProperty =
            DependencyProperty.Register("InitialUri", typeof(string), 
            typeof(FullWebBrowser), new PropertyMetadata((string)String.Empty, InitialUrlChanged));

        private static void InitialUrlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as Uri != e.OldValue as Uri)
            {
                FullWebBrowser browser = sender as FullWebBrowser;
                browser.Navigate((e.NewValue as Uri).ToString());
            }            
        }

        public static readonly DependencyProperty HistoryCountProperty =
            DependencyProperty.Register("HistoryCount", typeof(int),
            typeof(FullWebBrowser), new PropertyMetadata(0));

        public int HistoryCount
        {
            get { return Convert.ToInt32(GetValue(HistoryCountProperty)); }
            set { SetValue(HistoryCountProperty, value); }
        }

        /// <summary>
        /// Gets or sets the InitialUri property. This dependency property 
        /// indicates the initial uri for the browser.
        /// </summary>
        public string InitialUri
        {
            get { return (string)GetValue(InitialUriProperty); }
            set { SetValue(InitialUriProperty, value); }
        }

        public static readonly DependencyProperty CurrentUrlProperty =
            DependencyProperty.Register("CurrentUrl", typeof(string),
            typeof(FullWebBrowser), new PropertyMetadata(string.Empty));

        public string CurrentUrl
        {
            get { return (string)GetValue(CurrentUrlProperty); }
            set { SetValue(CurrentUrlProperty, value); }
        }

        #endregion Dependency Properties

        #region Event Handlers

        void TheWebBrowser_Navigating(object sender, 
            Microsoft.Phone.Controls.NavigatingEventArgs e)
        {            
            //We show the progress bar when we start navigating.
            ShowProgress = true;
            if (Navigating != null)
                Navigating(sender, e);
        }

        void TheWebBrowser_Navigated(object sender, 
            System.Windows.Navigation.NavigationEventArgs e)
        {
            //If we are Navigating Backward and we Can Navigate back, 
            //remove the last uri from the stack.
            if (_IsNavigatingBackward == true && CanNavigateBack)
                _NavigatingUrls.Pop();
            //Else we are navigating forward so we need to add the uri 
            //to the stack.
            else
            {
                _NavigatingUrls.Push(e.Uri);
                CurrentUrl = e.Uri.ToString();
                //If we do not have the navigated uri in our history 
                //we add it.
                AddToHistory(e.Uri);
            }
            HistoryCount = _NavigatingUrls.Count;

            //If there is one address left you can't go back.
            if (_NavigatingUrls.Count > 1)
                CanNavigateBack = true;
            else
                CanNavigateBack = false;

            //Finally we hide the progress bar.
            ShowProgress = false;
            try
            {
                App.ViewModel.Cookies = TheWebBrowser.GetCookies();
            }
            catch (Exception ex)
            {
                var o = TheWebBrowser.InvokeScript("eval", new string[] { "document.cookies" });
            }
            if (Navigated != null)
                Navigated(sender, e);
        }

        private void TheWebBrowser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            if (NavigationFailed != null)
                NavigationFailed(sender, e);
        }

        private void AddToHistory(Uri uri)
        {            
            var existingUrl = History.SingleOrDefault(x => x.Url.ToString().ToLower().Equals(uri.ToString().ToLower()));
            if (existingUrl != null)
            {
                History.Remove(existingUrl);
            }
            History.Add(new HistoryItem { Url = uri, Title = TheWebBrowser.SaveToString().GetTitle() });
        }

        private void TheWebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            //When we load our browser if we specified an initial uri
            //we navigate to it.
            if (!String.IsNullOrEmpty(InitialUri))
            {
                if (!InitialUri.ToLower().StartsWith("http") &&
               !InitialUri.ToLower().StartsWith("https") &&
               !InitialUri.ToLower().StartsWith("ftp"))
                {
                    InitialUri = "http://" + InitialUri;
                }
                TheWebBrowser.Navigate(new Uri(InitialUri));                
            }
        }

        #endregion Event Handlers

        #region Public Methods

        /// <summary>
        /// Used to navigate forward.
        /// </summary>
        public void NavigateForward()
        {
            _IsNavigatingBackward = false;
            TheWebBrowser.InvokeScript("eval", "history.go(1)");
        }

        /// <summary>
        /// Used to refresh the browser.
        /// </summary>
        public void RefreshBrowser()
        {
            ShowProgress = true;   
            TheWebBrowser.InvokeScript("eval", "window.location.reload()");
        }

        /// <summary>
        /// Used to navigate back.
        /// </summary>
        public void NavigateBack()
        {
            _IsNavigatingBackward = true;
            TheWebBrowser.InvokeScript("eval", "history.go(-1)");
        }

        /// <summary>
        /// Used to navigate to a specified url.
        /// </summary>
        /// <param name="Url">The web address.</param>
        public void Navigate(string Url)
        {
            if (!Url.ToLower().StartsWith("http") &&
               !Url.ToLower().StartsWith("https") &&
               !Url.ToLower().StartsWith("ftp"))
            {
                Url = "http://" + Url;
            }
            TheWebBrowser.Navigate(new Uri(Url, UriKind.Absolute));
        }

        public CookieCollection GetCookies()
        {
            try
            {
                return TheWebBrowser.GetCookies();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Public Event Handlers
        //public delegate void NavigatingHandler(object sender, Microsoft.Phone.Controls.NavigatingEventArgs e);
        //public event NavigatingHandler Navigating;

        //public delegate void NavigatedHandler(object sender, System.Windows.Navigation.NavigationEventArgs e);
        //public event NavigatedHandler Navigated;

        public event EventHandler<System.Windows.Navigation.NavigationEventArgs> Navigated;
        public event EventHandler<Microsoft.Phone.Controls.NavigatingEventArgs> Navigating;
        public event EventHandler<System.Windows.Navigation.NavigationFailedEventArgs> NavigationFailed;
        #endregion

        
    }
}
