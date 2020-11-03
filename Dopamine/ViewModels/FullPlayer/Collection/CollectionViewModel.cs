﻿using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Alex;
using Dopamine.Core.Base;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Dopamine.Views.FullPlayer.Collection;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionViewModel : BindableBase
    {
        private int slideInFrom;
        private IRegionManager regionManager;
        private readonly string Setting_NameSpace = "Collection";

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        private GridLength _leftPaneGridLength;
        public GridLength LeftPaneWidth
        {
            get
            {
                return _leftPaneGridLength;
            }
            set
            {
                SetProperty<GridLength>(ref _leftPaneGridLength, value);
                //if (!value.IsStar)
                SettingsClient.Set<double>(Setting_NameSpace, "LeftPaneWidth", value.Value);
            }
        }

        public CollectionViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            eventAggregator.GetEvent<IsCollectionPageChanged>().Subscribe(tuple =>
            {
                this.NagivateToPage(tuple.Item1, tuple.Item2);
            });
            LeftPaneWidth = CollectionUtils.String2GridLength(SettingsClient.Get<string>(Setting_NameSpace, CollectionUtils.Setting_LeftPaneGridLength));
        }

        private void NagivateToPage(SlideDirection direction, CollectionPage page)
        {
            this.SlideInFrom = direction == SlideDirection.RightToLeft ? Constants.SlideDistance : -Constants.SlideDistance;

            switch (page)
            {
                case CollectionPage.Artists:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionArtists).FullName);
                    break;
                case CollectionPage.Genres:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionGenres).FullName);
                    break;
                case CollectionPage.Albums:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionAlbums).FullName);
                    break;
                case CollectionPage.Songs:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionTracks).FullName);
                    break;
                case CollectionPage.Playlists:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionPlaylists).FullName);
                    break;
                case CollectionPage.Folders:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionFolders).FullName);
                    break;
                case CollectionPage.History:
                    this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionHistoryLog).FullName);
                    // ALEX TODO. Re-enable this in the same tab // this.regionManager.RequestNavigate(RegionNames.CollectionRegion, typeof(CollectionHistory).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
