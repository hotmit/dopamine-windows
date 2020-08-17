﻿using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Base;
using Dopamine.Services.Entities;
using Dopamine.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dopamine.Views.Common.Base
{
    public abstract class TracksViewBase : CommonViewBase
    {
        protected override async Task KeyUpHandlerAsync(object sender, KeyEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            if (e.Key == Key.J && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await this.ScrollToPlayingTrackAsync(lb);

            }
            else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (lb.SelectedItem != null)
                {
                    Actions.TryViewInExplorer(((TrackViewModel)lb.SelectedItem).Track.Path);
                }
            }
        }

        protected override async Task ActionHandler(Object sender, DependencyObject source, bool enqueue)
        {
            try
            {
                // Check if an item is selected
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem == null)
                {
                    return;
                }

                // Confirm that the user double clicked a valid item (and not on the scrollbar for example)
                if (source == null)
                {
                    return;
                }

                while (source != null && !(source is MultiSelectListBox.MultiSelectListBoxItem))
                {
                    source = VisualTreeHelper.GetParent(source);
                }

                if (source == null || source.GetType() != typeof(MultiSelectListBox.MultiSelectListBoxItem))
                {
                    return;
                }

                // The user double clicked a valid item
                if (!enqueue)
                {
                    // The user just wants to play the selected item. Don't enqueue.
                    if (lb.SelectedItem.GetType().Name == typeof(TrackViewModel).Name)
                    {
                        await this.playbackService.PlaySelectedAsync((TrackViewModel)lb.SelectedItem);
                    }

                    return;
                };

                // The user wants to enqueue tracks for the selected item
                if (lb.SelectedItem.GetType().Name == typeof(TrackViewModel).Name)
                {
                    await this.playbackService.EnqueueAsync(lb.Items.OfType<TrackViewModel>().ToList(), (TrackViewModel)lb.SelectedItem);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(ArtistViewModel).Name)
                {
                    await this.playbackService.EnqueueArtistsAsync(new List<string> { ((ArtistViewModel)lb.SelectedItem).Name }, false, false);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(GenreViewModel).Name)
                {
                    await this.playbackService.EnqueueGenresAsync(new List<string> { ((GenreViewModel)lb.SelectedItem).GenreName }, false, false);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(AlbumViewModel).Name)
                {
                    await this.playbackService.EnqueueAlbumsAsync(new List<AlbumViewModel> { (AlbumViewModel)lb.SelectedItem }, false, false);
                }
                else if (lb.SelectedItem.GetType().Name == typeof(PlaylistViewModel).Name)
                {
                    await this.playbackService.EnqueuePlaylistsAsync(new List<PlaylistViewModel> { (PlaylistViewModel)lb.SelectedItem }, false, false);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while handling action. Exception: {0}", ex.Message);
            }
        }

        protected override async Task ScrollToPlayingTrackAsync(Object sender)
        {
            try
            {
                // Cast sender to ListBox
                ListBox lb = (ListBox)sender;

                // This should provide a smoother experience because after this wait,
                // other animations on the UI should have finished executing.
                await Task.Delay(Convert.ToInt32(Constants.ScrollToPlayingTrackTimeoutSeconds * 1000));

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToPlayingTrackAsync(lb);
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not scroll to the playing track. Exception: {0}", ex.Message);
            }
        }

        protected override void ViewInExplorer(Object sender)
        {
            try
            {
                // Cast sender to ListBox
                ListBox lb = (ListBox)sender;

                if (lb.SelectedItem != null)
                {
                    Actions.TryViewInExplorer(((TrackViewModel)lb.SelectedItem).Track.Path);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not view track in Windows Explorer. Exception: {0}", ex.Message);
            }
        }
    }
}
