﻿using GoogleAnalytics;
using QuickShare.Classes;
using QuickShare.Classes.ItemSources;
using QuickShare.HelperClasses;
using QuickShare.ViewModels.History;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace QuickShare
{
    public sealed partial class HistoryPage : Page
    {
        public IncrementalLoadingCollection<HistoryItemSource, HistoryItem> HistoryItems { get; set; }

        public HistoryPage()
        {
            this.InitializeComponent();
            HistoryItems = new IncrementalLoadingCollection<HistoryItemSource, HistoryItem>(20, 1);
            HistoryItems.LoadFinished += HistoryItems_LoadFinished;
        }

        private void HistoryItems_LoadFinished(EventArgs e)
        {
            if (HistoryItems.Count == 0)
                HistoryEmptyNotice.Visibility = Visibility.Visible;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            App.Tracker.Send(HitBuilder.CreateScreenView("HistoryPage").Build());
#endif
        }

        private async void CopyToClipboard_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                DataPackage content = new DataPackage();
                content.SetText(((HyperlinkButton)sender).Tag.ToString());

                Clipboard.SetContent(content);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageDialog md = new MessageDialog(ex.Message, "Can't write to clipboard.");
                await md.ShowAsync();
            }
        }

        private async void OpenSingleFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var info = (ViewModels.History.FileInfo)(((Button)sender).Tag);

                await LaunchOperations.LaunchFileFromPathAsync(info.Path, info.FileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                await (new MessageDialog("File not found.")).ShowAsync();
            }
        }

        private async void OpenSingleFileContainingFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var info = (ViewModels.History.FileInfo)(((Button)sender).Tag);

                await LaunchOperations.LaunchFolderFromPathAndSelectSingleItemAsync(info.Path, info.FileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                await (new MessageDialog("File or folder does not exist.")).ShowAsync();
            }
        }

        private async void OpenFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await LaunchOperations.LaunchFolderFromPathAsync(((HyperlinkButton)sender).Tag.ToString());
            }
            catch (System.IO.FileNotFoundException)
            {
                await (new MessageDialog("File or folder does not exist.")).ShowAsync();
            }
        }

        private async void OpenLink_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var path = ((HyperlinkButton)sender).Tag.ToString();

            if (Uri.TryCreate(path, UriKind.Absolute, out Uri uri))
            {
                await Launcher.LaunchUriAsync(uri);
            }
            else
            {
                await (new MessageDialog("Invalid url.", "Can't open link")).ShowAsync();
            }
        }
    }
}
