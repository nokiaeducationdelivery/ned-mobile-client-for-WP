/*******************************************************************************
* Copyright (c) 2011 Nokia Corporation
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* Comarch team - initial API and implementation
*******************************************************************************/
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace NedWp
{
    public class DesiredOrientationEventArgs : EventArgs
    {
        public PageOrientation DesiredOrientation { get; set; }
    }

    public partial class MediaPlayerControl : UserControl
    {
        private DispatcherTimer ProgressUpdateTimer = new DispatcherTimer();
        private int CurrentPositionMiliseconds { get; set; }

        public event EventHandler<DesiredOrientationEventArgs> DesiredOrientationChanged;

        public static readonly DependencyProperty MediaSourceProperty = DependencyProperty.Register("MediaSource", typeof(string), typeof(MediaPlayerControl), new PropertyMetadata(new PropertyChangedCallback(MediaPlayerControl.OnMediaSourceChanged)));
        public string MediaSource
        {
            get { return (string)GetValue(MediaSourceProperty); }
            set
            {
                if (MediaSource != value)
                    SetValue(MediaSourceProperty, value);
            }
        }

        public static readonly DependencyProperty IsAudioProperty = DependencyProperty.Register("IsAudio", typeof(bool), typeof(MediaPlayerControl), new PropertyMetadata(false));
        public bool IsAudio
        {
            get { return (bool)GetValue(IsAudioProperty); }
            set { SetValue(IsAudioProperty, value); }
        }
        
        public static readonly DependencyProperty IsFullScreenDesiredProperty = DependencyProperty.Register("IsFullScreenDesired", typeof(bool), typeof(MediaPlayerControl), new PropertyMetadata(false));
        public bool IsFullScreenDesired
        {
            get { return (bool)GetValue(IsFullScreenDesiredProperty); }
            set { SetValue(IsFullScreenDesiredProperty, value); }
        }

        public static readonly DependencyProperty PlayPauseIconProperty = DependencyProperty.Register("PlayPauseIcon", typeof(string), typeof(MediaPlayerControl), null);
        public string PlayPauseIcon
        {
            get { return (string)GetValue(PlayPauseIconProperty); }
            set { SetValue(PlayPauseIconProperty, value); }
        }

        public MediaPlayerControl()
        {
            InitializeComponent();
            
            PlayPauseControl.DataContext = this;
            FullScreenToggler.DataContext = this;
            MediaControlPanel.DataContext = this;
            MediaPlayerElement.CurrentStateChanged += UpdatePlayPauseIcon;

            AutostartOnMediaLoad = true;

            MediaPlayerElement.MediaOpened += new RoutedEventHandler(OnMediaOpened);
            MediaPlayerElement.MediaEnded += new RoutedEventHandler(OnMediaEnded);
            MediaPlayerElement.Unloaded += new RoutedEventHandler(OnUnloaded);
            MediaPlayerElement.CurrentStateChanged += new RoutedEventHandler(OnMediaCurrentStateChanged);

            ProgressUpdateTimer.Interval = TimeSpan.FromMilliseconds(250);
            ProgressUpdateTimer.Tick += new EventHandler(OnCurrentMediaProgressUpdateRequested);

            UpdateTrackTimeDisplay();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ProgressUpdateTimer.Stop();
        }

        private static void OnMediaSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            MediaPlayerControl control = sender as MediaPlayerControl;
            control.LoadSource();
        }

        private void UpdatePlayPauseIcon(object sender, RoutedEventArgs args)
        {
            switch (MediaPlayerElement.CurrentState)
            {
                case MediaElementState.Playing:
                    PlayPauseIcon = IsAudio ? "../Resources/Icons/music_icon_pause_221x221.png" : "../Resources/Icons/pause_circle.png;";
                    break;
                case MediaElementState.Stopped:
                case MediaElementState.Paused:
                case MediaElementState.Closed:
                    PlayPauseIcon = IsAudio ? "../Resources/Icons/music_icon_play_221x221.png" : "../Resources/Icons/play_circle.png";
                    break;
                case MediaElementState.Opening:
                case MediaElementState.Individualizing:
                case MediaElementState.AcquiringLicense:
                case MediaElementState.Buffering:
                default:
                    PlayPauseIcon = String.Empty;
                    break;
            }
        }
        

        private void LoadSource()
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(MediaSource, FileMode.Open, FileAccess.Read))
                {
                    MediaPlayerElement.SetSource(fileStream);
                }
            }
        }

        private void OnPlayPauseButtonClicked(object sender, ManipulationCompletedEventArgs args)
        {
            if (MediaPlayerElement.CurrentState == MediaElementState.Playing)
                MediaPlayerElement.Pause();
            else
                MediaPlayerElement.Play();
        }

        private void OnMediaCurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (MediaPlayerElement.CurrentState == MediaElementState.Opening ||
                MediaPlayerElement.CurrentState == MediaElementState.Playing ||
                MediaPlayerElement.CurrentState == MediaElementState.Buffering)
            {
                if (!IsAudio)
                    MediaControlPanel.Visibility = Visibility.Collapsed;
                ProgressUpdateTimer.Start();
            }
            else
            {
                MediaControlPanel.Visibility = Visibility.Visible;
                ProgressUpdateTimer.Stop();
            }
        }

        // WARN: This flag was added to avoid 'always run on start behaviour'. 
        // Even when going back from Links page video moved to start and played.
        // Not sure if it won't cause some issues with tombstoning...
        private bool AutostartOnMediaLoad { get; set; }
        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            MediaProgressBar.Maximum = (int)MediaPlayerElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            ProgressUpdateTimer.Stop();
            MediaControlPanel.Visibility = Visibility.Visible;
            if (AutostartOnMediaLoad)
            {
                AutostartOnMediaLoad = false;
                MediaPlayerElement.Play();
            }
            else // Workaround: for issue with video beeing reset on page forward/back navigation
            {
                MediaPlayerElement.Pause(); // To enable changing position
                GoToPosition(CurrentPositionMiliseconds);
                UpdateTrackTimeDisplay();
            }
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            ProgressUpdateTimer.Stop();
            UpdateTrackTimeDisplay();
            CurrentPositionMiliseconds = 0;
            MediaProgressBar.Value = 0;
            MediaPlayerElement.Stop();
        }

        private void OnCurrentMediaProgressUpdateRequested(object sender, EventArgs e)
        {
            int curMs = (int)MediaPlayerElement.Position.TotalMilliseconds;
            CurrentPositionMiliseconds = curMs != 0 ? curMs : CurrentPositionMiliseconds;
            MediaProgressBar.Value = curMs;
            UpdateTrackTimeDisplay();
        }

        private void UpdateTrackTimeDisplay()
        {
            DateTime leftTime = new DateTime(MediaPlayerElement.Position.Ticks);
            DateTime rightTime = new DateTime(MediaPlayerElement.NaturalDuration.TimeSpan.Ticks);
            if (leftTime == null || rightTime == null)
            {
                leftTime = new DateTime();
                rightTime = leftTime;
            }
            ProgressTimeDisplay.Text = String.Format("{0}/{1}", leftTime.ToString("mm:ss"), rightTime.ToString("mm:ss"));
        }

        private void OnProgressBarClicked(object sender, ManipulationCompletedEventArgs args)
        {
            if ((sender as FrameworkElement).ActualWidth <= 0 && MediaPlayerElement.CanSeek)
                return;
            int miliseconds = (int)((args.ManipulationOrigin.X / (sender as FrameworkElement).ActualWidth) * MediaProgressBar.Maximum);
            CurrentPositionMiliseconds = miliseconds;
            GoToPosition(miliseconds);
        }

        private void GoToPosition(int miliseconds)
        {
            MediaProgressBar.Value = miliseconds;
            MediaPlayerElement.Position = TimeSpan.FromMilliseconds(miliseconds);
        }

        private void OnToggleScreenClicked(object sender, ManipulationCompletedEventArgs args)
        {
            ToggleFullScreen();
        }

        private void ToggleFullScreen()
        {
            PhoneApplicationPage page = (App.Current.RootVisual as PhoneApplicationFrame).Content as PhoneApplicationPage;
            if (page.Orientation == PageOrientation.Landscape || page.Orientation == PageOrientation.LandscapeLeft || page.Orientation == PageOrientation.LandscapeRight)
            {
                IsFullScreenDesired = false;
                BroadcastDesiredOrientationChanged(PageOrientation.Portrait);
            }
            else
            {
                IsFullScreenDesired = true;
                BroadcastDesiredOrientationChanged(PageOrientation.Landscape);
            }
        }

        protected virtual void BroadcastDesiredOrientationChanged(PageOrientation desiredOrientation)
        {
            if (DesiredOrientationChanged != null)
                DesiredOrientationChanged(this, new DesiredOrientationEventArgs() { DesiredOrientation = desiredOrientation });
        }
    }

    public class PlayIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage image = new BitmapImage(new Uri((bool)value ? "../Resources/Icons/music_icon_221x221.png" : "../Resources/OriginalPlatformIcons/appbar.transport.play.rest.png", UriKind.Relative));
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FullscreenTogglerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FullscreenTogglerIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible) 
                return (bool)value ? "../Resources/Icons/toggle-to-portrait.png" : "../Resources/Icons/toggle-to-fullscreen.png";
            else
                return (bool)value ? "../Resources/Icons/toggle-to-portrait-black.png" : "../Resources/Icons/toggle-to-fullscreen-black.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}