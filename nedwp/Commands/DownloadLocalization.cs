using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NedEngine;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Reactive;


namespace NedWp.Commands
{
    public class DownloadLocalization : ICommand
    {

        private static DownloadLocalization mInstance = null;

        public static DownloadLocalization GetCommand()
        {
            if (mInstance == null)
                mInstance = new DownloadLocalization();
            return mInstance;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }


        public void Execute(object parameter)
        {
            LanguageInfo languageToDownload = parameter as LanguageInfo;

            switch (languageToDownload.ItemState)
            {
                case MediaItemState.Local:
                    // select language
                    App.Engine.ApplicationSettings.AvailableLanguages.CurrentLanguage = languageToDownload.Id;
                    App.Engine.ApplicationSettings.AvailableLanguages.Save(false);
                    break;
                case MediaItemState.Downloading:
                    ToastPrompt toast = new ToastPrompt();
                    toast.Message = String.Format("{0} is already queued for download", languageToDownload.LangName == String.Empty ? "Item" : languageToDownload.LangName);
                    toast.Show();
                    break;
                case MediaItemState.Remote:
                    languageToDownload.ItemState = MediaItemState.Downloading;
                    App.Engine.DownloadLocalization(languageToDownload.Id)
                        .ObserveOnDispatcher()
                        .Finally( () =>
                            {
                            })
                        .Subscribe<bool>( success => 
                            {
                                if (success)
                                {
                                    languageToDownload.IsLocal = true;
                                    App.Engine.ApplicationSettings.AvailableLanguages.Save(false);
                                }
                                else
                                {
                                    languageToDownload.ItemState = MediaItemState.Remote;
                                }
                            });
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "Unknown language item download state");
                    break;
            }
        }

        private IObservable<Unit> savelanguageFile()
        {
            return Observable.Empty<Unit>();
        }
    }
}
