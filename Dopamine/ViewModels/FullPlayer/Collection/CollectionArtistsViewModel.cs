﻿using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Alex; //Digimezzo.Foundation.Core.Settings
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Prism;
using Dopamine.Data;
using Dopamine.Services.Collection;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Indexing;
using Dopamine.Services.Playback;
using Dopamine.Services.Playlist;
using Dopamine.Services.Search;
using Dopamine.Services.Utils;
using Dopamine.ViewModels.Common.Base;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Dopamine.Data.Entities;
using System.Diagnostics;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionArtistsViewModel : AlbumsViewModelBase, ISemanticZoomViewModel
    {
        private ICollectionService collectionService;
        private IPlaybackService playbackService;
        private IPlaylistService playlistService;
        private IIndexingService indexingService;
        private IDialogService dialogService;
        private ISearchService searchService;
        private IEventAggregator eventAggregator;
        private ObservableCollection<ISemanticZoomable> artists;
        private CollectionViewSource artistsCvs;
        private IList<ArtistViewModel> selectedArtists;
        private ObservableCollection<ISemanticZoomSelector> artistsZoomSelectors;
        private bool isArtistsZoomVisible;
        private long artistsCount;
        private double leftPaneWidthPercent;
        private double rightPaneWidthPercent;
        private ArtistType artistType;
        private string artistTypeText;
        private IList<long> selectedArtistIDs;

        public DelegateCommand<string> AddArtistsToPlaylistCommand { get; set; }


        public DelegateCommand<object> SelectedArtistsCommand { get; set; }

        public DelegateCommand ShowArtistsZoomCommand { get; set; }

        public DelegateCommand<string> SemanticJumpCommand { get; set; }

        public DelegateCommand AddArtistsToNowPlayingCommand { get; set; }

        public DelegateCommand ShuffleSelectedArtistsCommand { get; set; }

        public DelegateCommand<ArtistViewModel> PlayArtistCommand { get; set; }
        public DelegateCommand<ArtistViewModel> EnqueueArtistCommand { get; set; }
        public DelegateCommand<ArtistViewModel> LoveArtistCommand { get; set; }

        public ArtistType ArtistType
        {
            get { return this.artistType; }
            set
            {
                SetProperty<ArtistType>(ref this.artistType, value);
                this.UpdateArtistTypeText(value);
            }
        }

        public string ArtistTypeText
        {
            get { return this.artistTypeText; }
        }

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "ArtistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public double RightPaneWidthPercent
        {
            get { return this.rightPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.rightPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "ArtistsRightPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public ObservableCollection<ISemanticZoomable> Artists
        {
            get { return this.artists; }
            set { SetProperty<ObservableCollection<ISemanticZoomable>>(ref this.artists, value); }
        }

        ObservableCollection<ISemanticZoomable> ISemanticZoomViewModel.SemanticZoomables
        {
            get { return Artists; }
            set { Artists = value; }
        }

        public CollectionViewSource ArtistsCvs
        {
            get { return this.artistsCvs; }
            set { SetProperty<CollectionViewSource>(ref this.artistsCvs, value); }
        }

        public IList<ArtistViewModel> SelectedArtists
        {
            get { return this.selectedArtists; }
            set { SetProperty<IList<ArtistViewModel>>(ref this.selectedArtists, value); }
        }


        public long ArtistsCount
        {
            get { return this.artistsCount; }
            set { SetProperty<long>(ref this.artistsCount, value); }
        }

        public bool IsArtistsZoomVisible
        {
            get { return this.isArtistsZoomVisible; }
            set { SetProperty<bool>(ref this.isArtistsZoomVisible, value); }
        }

        public ObservableCollection<ISemanticZoomSelector> ArtistsZoomSelectors
        {
            get { return this.artistsZoomSelectors; }
            set { SetProperty<ObservableCollection<ISemanticZoomSelector>>(ref this.artistsZoomSelectors, value); }
        }
        ObservableCollection<ISemanticZoomSelector> ISemanticZoomViewModel.SemanticZoomSelectors
        {
            get { return ArtistsZoomSelectors; }
            set { ArtistsZoomSelectors = value; }
        }

        public bool HasSelectedArtists
        {
            get
            {
                return (this.SelectedArtists != null && this.SelectedArtists.Count > 0);
            }
        }

        public CollectionArtistsViewModel(IContainerProvider container) : base(container)
        {
            // Dependency injection
            this.collectionService = container.Resolve<ICollectionService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.playlistService = container.Resolve<IPlaylistService>();
            this.indexingService = container.Resolve<IIndexingService>();
            this.dialogService = container.Resolve<IDialogService>();
            this.searchService = container.Resolve<ISearchService>();
            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Commands
            this.ToggleTrackOrderCommand = new DelegateCommand(async () => await this.ToggleTrackOrderAsync());
            this.ToggleAlbumOrderCommand = new DelegateCommand(async () => await this.ToggleAlbumOrderAsync());
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.AddArtistsToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddArtistsToPlaylistAsync(this.SelectedArtists, playlistName));
            this.SelectedArtistsCommand = new DelegateCommand<object>(async (parameter) => await this.SelectedArtistsHandlerAsync(parameter));
            this.ShowArtistsZoomCommand = new DelegateCommand(async () => await this.ShowSemanticZoomAsync());
            this.AddArtistsToNowPlayingCommand = new DelegateCommand(async () => await this.AddArtistsToNowPlayingAsync(this.SelectedArtists));
            this.ShuffleSelectedArtistsCommand = new DelegateCommand(async () =>
            {
                await this.playbackService.PlayArtistsAsync(SelectedArtists, PlaylistMode.Play, true);
            });
            this.PlayArtistCommand = new DelegateCommand<ArtistViewModel>(async (avm) => {
                await this.playbackService.PlayArtistsAsync(new List<ArtistViewModel>() { avm }, PlaylistMode.Play);
            });
            this.EnqueueArtistCommand = new DelegateCommand<ArtistViewModel>(async (avm) => await this.playbackService.PlayArtistsAsync(new List<ArtistViewModel>() { avm }, PlaylistMode.Enqueue));
            this.LoveArtistCommand = new DelegateCommand<ArtistViewModel>((avm) => Debug.Assert(false, "ALEX TODO"));



            this.SemanticJumpCommand = new DelegateCommand<string>((header) =>
            {
                this.HideSemanticZoom();
                this.eventAggregator.GetEvent<PerformSemanticJump>().Publish(new Tuple<string, string>("Artists", header));
            });

            // Settings
            Digimezzo.Foundation.Core.Settings.SettingsClient.SettingChanged += async (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableRating"))
                {
                    this.EnableRating = (bool)e.Entry.Value;
                    this.SetTrackOrder("ArtistsTrackOrder");
                    await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbums, this.TrackOrder);
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableLove"))
                {
                    this.EnableLove = (bool)e.Entry.Value;
                    this.SetTrackOrder("ArtistsTrackOrder");
                    await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbums, this.TrackOrder);
                }

                if (SettingsClient.IsSettingChanged(e, "State", "SelectedArtistIDs"))
                {
                    LoadSelectedArtists();
                }

            };

            // PubSub Events
            this.eventAggregator.GetEvent<ShellMouseUp>().Subscribe((_) => this.IsArtistsZoomVisible = false);
            this.eventAggregator.GetEvent<ToggleArtistOrderCommand>().Subscribe((_) => this.ToggleArtistTypeAsync());

            // Set the initial ArtistOrder			
            this.ArtistType = (ArtistType)SettingsClient.Get<int>("Ordering", "ArtistsArtistType");

            // Set the initial AlbumOrder
            this.AlbumOrder = (AlbumOrder)SettingsClient.Get<int>("Ordering", "ArtistsAlbumOrder");

            // Set the initial TrackOrder
            this.SetTrackOrder("ArtistsTrackOrder");

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "ArtistsLeftPaneWidthPercent");
            this.RightPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "ArtistsRightPaneWidthPercent");

            // Cover size
            this.SetCoversizeAsync((CoverSizeType)SettingsClient.Get<int>("CoverSizes", "ArtistsCoverSize"));
            LoadSelectedArtists();

        }

        private void LoadSelectedArtists()
        {
            try
            {
                string s = SettingsClient.Get<String>("State", "SelectedArtistIDs");
                if (!string.IsNullOrEmpty(s))
                {
                    selectedArtistIDs = s.Split(',').Select(x => long.Parse(x)).ToList();
                    return;
                }
            }
            catch (Exception _)
            {

            }
            selectedArtistIDs = new List<long>();
        }

        private void SaveSelectedArtists()
        {
            string s = string.Join(",", selectedArtistIDs);// SettingsClient.Get<String>("State", "SelectedArtistIDs");
            SettingsClient.Set<String>("State", "SelectedArtistIDs", s);
        }

        public async Task ShowSemanticZoomAsync()
        {
            this.ArtistsZoomSelectors = await SemanticZoomUtils.UpdateSemanticZoomSelectors(this.ArtistsCvs.View);
            this.IsArtistsZoomVisible = true;
        }

        public void HideSemanticZoom()
        {
            this.IsArtistsZoomVisible = false;
        }

        public void UpdateSemanticZoomHeaders()
        {
            string previousHeader = string.Empty;

            foreach (ArtistViewModel avm in this.ArtistsCvs.View)
            {
                if (string.IsNullOrEmpty(previousHeader) || !avm.Header.Equals(previousHeader))
                {
                    previousHeader = avm.Header;
                    avm.IsHeader = true;
                }
                else
                {
                    avm.IsHeader = false;
                }
            }
        }

        private void SetArtistOrder(string settingName)
        {
            this.ArtistType = (ArtistType)SettingsClient.Get<int>("Ordering", settingName);
        }

        private void ClearArtists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.ArtistsCvs != null)
                {
                    this.ArtistsCvs.Filter -= new FilterEventHandler(ArtistsCvs_Filter);
                }

                this.ArtistsCvs = null;
            });

            this.Artists = null;
        }

        private async Task GetArtistsAsync(ArtistType artistType)
        {
            try
            {
                // Get the artists
                var artistViewModels = new ObservableCollection<ArtistViewModel>(await this.collectionService.GetAllArtistsAsync(artistType));

                // Unbind to improve UI performance
                ClearArtists();
                selectedArtists = new List<ArtistViewModel>();
                foreach (long id in selectedArtistIDs)
                {
                    ArtistViewModel avm = artistViewModels.Where(x => x.Id == id).FirstOrDefault();
                    if (avm != null)
                    {
                        avm.IsSelected = selectedArtistIDs.Contains(avm.Id);
                        selectedArtists.Add(avm);
                    }
                }
                /*
                foreach (ArtistViewModel avm in artistViewModels)
                {
                    if (selectedArtistIDs.Contains(avm.Id))
                    {
                        avm.IsSelected = selectedArtistIDs.Contains(avm.Id);
                        selectedArtists.Add(avm);
                    }
                }
                */

                // Populate ObservableCollection
                Artists = new ObservableCollection<ISemanticZoomable>(artistViewModels);
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occurred while getting Artists. Exception: {0}", ex.Message);

                // Failed getting Artists. Create empty ObservableCollection.
                Artists = new ObservableCollection<ISemanticZoomable>();
            }



            Application.Current.Dispatcher.Invoke(() =>
            {
                // Populate CollectionViewSource
                this.ArtistsCvs = new CollectionViewSource { Source = Artists };
                this.ArtistsCvs.Filter += new FilterEventHandler(ArtistsCvs_Filter);

                // Update count
                this.ArtistsCount = Artists.Count;
            });

            // Update Semantic Zoom Headers
            this.UpdateSemanticZoomHeaders();
        }

        private async Task SelectedArtistsHandlerAsync(object parameter)
        {
            bool bKeepOldSelections = true;
            if (parameter != null && ((IList)parameter).Count > 0)
            {
                if (((IList)parameter).Count > 0)
                {
                    bKeepOldSelections = false;
                    selectedArtistIDs.Clear();
                    selectedArtists.Clear();


                    foreach (ArtistViewModel item in (IList)parameter)
                    {
                        selectedArtists.Add(item);
                        item.IsSelected = true;
                        selectedArtistIDs.Add(item.Id);
                    }

                }
            }
            
            if (bKeepOldSelections)
            {
                // Keep the previous selection if possible. Otherwise select All
                List<long> validSelectedArtistIDs = new List<long>();
                selectedArtists.Clear();
                foreach (long id in selectedArtistIDs)
                {
                    ArtistViewModel sel = (ArtistViewModel) artists.Where(x => ((ArtistViewModel)x).Id == id).FirstOrDefault();
                    if (sel != null)
                    {
                        validSelectedArtistIDs.Add(id);
                        sel.IsSelected = true;
                        selectedArtists.Add(sel);
                    }
                }
                selectedArtistIDs = validSelectedArtistIDs;

            }

            this.RaisePropertyChanged(nameof(this.HasSelectedArtists));
            Task saveSelectedArtists = Task.Run(() => SaveSelectedArtists());
            Task albums = GetArtistAlbumsAsync(selectedArtists, this.ArtistType, this.AlbumOrder);
            this.SetTrackOrder("ArtistsTrackOrder");
            Task tracks = GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbums, this.TrackOrder);
            Task.WhenAll(albums, tracks, saveSelectedArtists);

        }

        private async Task AddArtistsToPlaylistAsync(IList<ArtistViewModel> artists, string playlistName)
        {
            CreateNewPlaylistResult addPlaylistResult = CreateNewPlaylistResult.Success; // Default Success

            // If no playlist is provided, first create one.
            if (playlistName == null)
            {
                var responseText = ResourceUtils.GetString("Language_New_Playlist");

                if (this.dialogService.ShowInputDialog(
                    0xea37,
                    16,
                    ResourceUtils.GetString("Language_New_Playlist"),
                    ResourceUtils.GetString("Language_Enter_Name_For_Playlist"),
                    ResourceUtils.GetString("Language_Ok"),
                    ResourceUtils.GetString("Language_Cancel"),
                    ref responseText))
                {
                    playlistName = responseText;
                    addPlaylistResult = await this.playlistService.CreateNewPlaylistAsync(new EditablePlaylistViewModel(playlistName, PlaylistType.Static));
                }
            }

            // If playlist name is still null, the user clicked cancel on the previous dialog. Stop here.
            if (playlistName == null) return;

            // Verify if the playlist was added
            switch (addPlaylistResult)
            {
                case CreateNewPlaylistResult.Success:
                case CreateNewPlaylistResult.Duplicate:
                    // Add items to playlist
                    AddTracksToPlaylistResult result = await this.playlistService.AddArtistsToStaticPlaylistAsync(artists, playlistName);

                    if (result == AddTracksToPlaylistResult.Error)
                    {
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Adding_Songs_To_Playlist").Replace("{playlistname}", "\"" + playlistName + "\""), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
                    }
                    break;
                case CreateNewPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                    break;
                case CreateNewPlaylistResult.Blank:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetString("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                default:
                    // Never happens
                    break;
            }
        }

        private void ArtistsCvs_Filter(object sender, FilterEventArgs e)
        {
            ArtistViewModel avm = e.Item as ArtistViewModel;

            e.Accepted = Services.Utils.EntityUtils.FilterArtists(avm, this.searchService.SearchText);
        }

        private async Task AddArtistsToNowPlayingAsync(IList<ArtistViewModel> artists)
        {
            await this.playbackService.PlayArtistsAsync(artists, PlaylistMode.Enqueue);
        }

        private async Task ToggleArtistTypeAsync()
        {
            this.HideSemanticZoom();

            switch (this.ArtistType)
            {
                case ArtistType.All:
                    this.ArtistType = ArtistType.Track;
                    break;
                case ArtistType.Track:
                    this.ArtistType = ArtistType.Album;
                    break;
                case ArtistType.Album:
                    this.ArtistType = ArtistType.All;
                    break;
                default:
                    // Cannot happen, but just in case.	
                    this.ArtistType = ArtistType.All;
                    break;
            }

            SettingsClient.Set<int>("Ordering", "ArtistsArtistType", (int)this.ArtistType);
            await this.GetArtistsAsync(this.ArtistType);
        }

        private void UpdateArtistTypeText(ArtistType artistType)
        {
            switch (artistType)
            {
                case ArtistType.All:
                    this.artistTypeText = ResourceUtils.GetString("Language_All_Artists");
                    break;
                case ArtistType.Track:
                    this.artistTypeText = ResourceUtils.GetString("Language_Song_Artists");
                    break;
                case ArtistType.Album:
                    this.artistTypeText = ResourceUtils.GetString("Language_Album_Artists");
                    break;
                default:
                    // Cannot happen, but just in case.	
                    this.artistTypeText = ResourceUtils.GetString("Language_All_Artists");
                    break;
            }

            RaisePropertyChanged(nameof(this.ArtistTypeText));
        }

        private async Task ToggleTrackOrderAsync()
        {
            base.ToggleTrackOrder();

            SettingsClient.Set<int>("Ordering", "ArtistsTrackOrder", (int)this.TrackOrder);
            await this.GetTracksCommonAsync(this.Tracks, this.TrackOrder);
        }

        private async Task ToggleAlbumOrderAsync()
        {

            base.ToggleAlbumOrder();

            SettingsClient.Set<int>("Ordering", "ArtistsAlbumOrder", (int)this.AlbumOrder);
            await this.GetAlbumsCommonAsync(this.Albums, this.AlbumOrder);
        }

        protected async override Task SetCoversizeAsync(CoverSizeType coverSize)
        {
            await base.SetCoversizeAsync(coverSize);
            SettingsClient.Set<int>("CoverSizes", "ArtistsCoverSize", (int)coverSize);
        }
        /* USAGE FOR A SINGLE SELECTED ITEM
         * in CollectionArtists.xaml:                                 
         * <dc:MultiSelectListBox ...  SelectedItem="{Binding SelectedArtist, Mode=TwoWay}" ...
         * Here:
            public ArtistViewModel SelectedArtist
            {
                get { return selectedArtists?.Count == 0 ? null : selectedArtists[0]; }
                set
                {
                    SetProperty<IList<ArtistViewModel>>(ref this.selectedArtists, new List<ArtistViewModel>() { value });
                }
            }
         */


        protected async override Task FillListsAsync()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await GetArtistsAsync(ArtistType);
                await GetArtistAlbumsAsync(this.SelectedArtists, this.ArtistType, this.AlbumOrder);
                await GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbums, this.TrackOrder);
                /*
                List<Task> tasks = new List<Task>();
                tasks.Add(GetArtistsAsync(ArtistType));
                tasks.Add(GetArtistAlbumsAsync(this.SelectedArtists, this.ArtistType, this.AlbumOrder));
                tasks.Add(GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbums, this.TrackOrder));
                Task.WhenAll(tasks.ToArray());
                */
            });
            

        }

        protected async override Task EmptyListsAsync()
        {
            this.ClearArtists();
            this.ClearAlbums();
            this.ClearTracks();
        }

        protected override void FilterLists()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Artists
                if (this.ArtistsCvs != null)
                {
                    this.ArtistsCvs.View.Refresh();
                    this.ArtistsCount = this.ArtistsCvs.View.Cast<ISemanticZoomable>().Count();
                    this.UpdateSemanticZoomHeaders();
                }
            });

            base.FilterLists();
        }

        protected async override Task SelectedAlbumsHandlerAsync(object parameter)
        {
            await base.SelectedAlbumsHandlerAsync(parameter);

            this.SetTrackOrder("ArtistsTrackOrder");
            await this.GetTracksAsync(this.SelectedArtists, null, this.SelectedAlbums, this.TrackOrder);
        }

        protected override void RefreshLanguage()
        {
            this.UpdateAlbumOrderText(this.AlbumOrder);
            this.UpdateTrackOrderText(this.TrackOrder);
            base.RefreshLanguage();
        }
    }
}
