﻿using Dopamine.Views.Common.Base;
using Dopamine.Core.Prism;
using Prism.Commands;
using System.Windows;
using System.Windows.Input;
using Dopamine.ViewModels.FullPlayer.Collection;
using Dopamine.Services.Entities;
using System.Windows.Controls;
using Digimezzo.Foundation.Core.Utils;

namespace Dopamine.Views.FullPlayer.Collection
{
    public partial class CollectionAlbums : TracksViewBase
    {
        public CollectionAlbums() : base()
        {
            InitializeComponent();

            // Commands
            this.ViewInExplorerCommand = new DelegateCommand(() => this.ViewInExplorer(this.ListBoxTracks));
            this.JumpToPlayingTrackCommand = new DelegateCommand(async () => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));

            // PubSub Events
            this.eventAggregator.GetEvent<ScrollToPlayingTrack>().Subscribe(async (_) => await this.ScrollToPlayingTrackAsync(this.ListBoxTracks));
            CollectionAlbumsViewModel vm = (CollectionAlbumsViewModel)DataContext;
            vm.EnsureItemVisible += (AlbumViewModel item) =>
            {
                ListBoxAlbums.ScrollIntoView(item);
            };
            vm.SelectionChanged += () =>
            {
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeUtils.GetDescendantByType(ListBoxTracks, typeof(ScrollViewer));
                scrollViewer?.ScrollToTop();
            };
        }

        private async void ListBoxAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxAlbums_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private async void ListBoxTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
        }

        private async void ListBoxTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.ActionHandler(sender, e.OriginalSource as DependencyObject, true);
            }
        }

        private async void ListBoxTracks_KeyUp(object sender, KeyEventArgs e)
        {
            await this.KeyUpHandlerAsync(sender, e);
        }

        private async void AlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListBoxAlbums.SelectedItem = null;
        }
    }
}
