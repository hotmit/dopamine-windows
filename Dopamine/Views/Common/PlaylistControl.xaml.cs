﻿using Dopamine.Views.Common.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using System.Windows;
using System.Windows.Input;
using Dopamine.ViewModels.Common;
using System.Windows.Controls;
using System.Threading.Tasks;
using System;
using Digimezzo.Foundation.WPF.Controls;
using System.Windows.Media;
using Dopamine.Services.Entities;

namespace Dopamine.Views.Common
{
    public partial class PlaylistControl : TracksViewBase
    {
        public PlaylistControl()
        {
            InitializeComponent();

            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // PubSub Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
        }
      
        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, false, false);
        }

        protected override async Task ActionHandler(Object sender, DependencyObject source, bool enqueue, bool includeTheRestOfTheList)
        {
            PlaylistControlViewModel vm = (PlaylistControlViewModel)this.DataContext;
            if (vm.InSearchMode)
            {
                await base.ActionHandler(sender, source, enqueue, false);
            }
            else
            {
                ListBox lb = (ListBox)sender;
                if (lb.SelectedItem == null)
                    return;
                if (source == null)
                    return;
                while (source != null && !(source is MultiSelectListBox.MultiSelectListBoxItem))
                {
                    source = VisualTreeHelper.GetParent(source);
                }
                if (source == null || source.GetType() != typeof(MultiSelectListBox.MultiSelectListBoxItem))
                    return;

                // The user just wants to play the selected item. Don't enqueue.
                if (lb.SelectedItem.GetType().Name == typeof(TrackViewModel).Name)
                {
                    await this.playbackService.SetPlaylistPositionAsync(lb.SelectedIndex);
                    //await this.playbackService.PlaySelectedAsync((TrackViewModel)lb.SelectedItem);
                }
            }
        }

        private void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            Task unAwaitedTask = this.KeyUpHandlerAsync(sender, e);
        }

        private void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Task unAwaitedTask = this.ActionHandler(sender, e.OriginalSource as DependencyObject, false, true);
            }
        }
    }
}
