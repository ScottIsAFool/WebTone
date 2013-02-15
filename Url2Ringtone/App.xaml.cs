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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Collections;
using Microsoft.Phone.Tasks;
using System.IO;
using Url2Ringtone.ViewModel;

namespace Url2Ringtone
{
    public partial class App : Application
    {
        private static MainViewModel viewModel = null;
        private static BrowserViewModel browserViewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = (MainViewModel)App.Current.Resources["viewModel"];

                return viewModel;
            }
        }

        public static BrowserViewModel BrowserModel
        {
            get
            {
                if (browserViewModel == null)
                    browserViewModel = ((Url2Ringtone.ViewModel.ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;                
                return browserViewModel;
            }
        }

        /// <summary>
        /// Gets the browser view model.
        /// </summary>
        

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }
        
        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            var bVM = ((ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;
            if (settings.Contains("History")) ViewModel.Items = (ObservableCollection<RingtoneItem>)settings["History"];
            if (settings.Contains("Favourites")) bVM.Favourites = (ObservableCollection<FavouriteItem>)settings["Favourites"];
            if (settings.Contains("BrowsingHistory")) bVM.History = (ObservableCollection<HistoryItem>)settings["BrowsingHistory"];
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
            {
                var bVM = ((ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;
                var state = PhoneApplicationService.Current.State;
                if (state.ContainsKey("ViewModel")) viewModel = (MainViewModel)state["ViewModel"];
                if (state.ContainsKey("BrowserViewModel")) bVM = (BrowserViewModel)state["BrowserViewModel"];                
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            var state = PhoneApplicationService.Current.State;
            var bVM = ((ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;
            state["ViewModel"] = App.ViewModel;
            state["BrowserViewModel"] = bVM;
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            // Ensure that required application state is persisted here.
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            var bVM = ((ViewModelLocator)App.Current.Resources["Locator"]).BrowserViewModel;
            settings["History"] = App.ViewModel.Items;
            settings["Favourites"] = bVM.Favourites;
            settings["BrowsingHistory"] = bVM.History;
            settings.Save();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            LittleWatson.ReportException(e.Exception, sender.ToString());
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
            e.Handled = true;
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            LittleWatson.ReportException(e.ExceptionObject, sender.ToString());
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            e.Handled = true;
        }        

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }

    public class LittleWatson
    {
        const string filename = "LittleWatson.txt";

        internal static void ReportException(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine(extra);
                        output.WriteLine(string.Format("URL: {0}", string.IsNullOrEmpty(App.ViewModel.CurrentItem.OriginalUrl) ? "no link" : App.ViewModel.CurrentItem.OriginalUrl));
                        output.WriteLine(string.Format("Format: {0}", string.IsNullOrEmpty(App.ViewModel.CurrentItem.FileExtension) ? "no extension" : App.ViewModel.CurrentItem.FileExtension));
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception exc)
            {
            }
        }

        internal static void CheckForPreviousException()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        SafeDeleteFile(store);
                    }
                }
                if (contents != null)
                {
                    if (MessageBox.Show("A problem occurred the last time you ran this application. Would you like to send an email to report it?", "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        EmailComposeTask email = new EmailComposeTask();
                        email.To = "scottisafool@live.co.uk";
                        email.Subject = "WebTone auto-generated problem report";
                        email.Body = contents;
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                        email.Show();
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception ex)
            {
            }
        }
    }
}