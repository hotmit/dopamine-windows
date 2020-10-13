﻿using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Dopamine.Data.Repositories;
using Dopamine.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer
{
    public class FullPlayerAddMusicViewModel : BindableBase
    {
        private bool checkBoxRefreshCollectionAutomaticallyChecked;
        private bool checkBoxIgnoreRemovedFilesChecked = false;
        private bool checkBoxDownloadMissingAlbumCoversChecked;
        private bool checkBoxRereadInfoChecked;
        private ITrackVRepository trackRepository;
        private IIndexingService indexingService;

        public FullPlayerAddMusicViewModel(ITrackVRepository trackRepository, IIndexingService indexingService)
        {
            this.trackRepository = trackRepository;
            this.indexingService = indexingService;
            this.RefreshNowCommand = new DelegateCommand(() => this.indexingService.RefreshCollectionAsync(true, checkBoxRereadInfoChecked));
            this.ReloadAllCoversCommand = new DelegateCommand(() => this.indexingService.RetrieveInfoAsync(true, true));
            this.ReloadMissingCoversCommand = new DelegateCommand(() => this.indexingService.RetrieveInfoAsync(true, false));
            this.GetCheckBoxesAsync();
        }

        public DelegateCommand RefreshNowCommand { get; set; }

        public DelegateCommand ReloadAllCoversCommand { get; set; }

        public DelegateCommand ReloadMissingCoversCommand { get; set; }

        public bool CheckBoxRefreshCollectionAutomaticallyChecked
        {
            get { return this.checkBoxRefreshCollectionAutomaticallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Indexing", "RefreshCollectionAutomatically", value, true);
                SetProperty<bool>(ref this.checkBoxRefreshCollectionAutomaticallyChecked, value);
            }
        }

        public bool CheckBoxIgnoreRemovedFilesChecked
        {
            get { return this.checkBoxIgnoreRemovedFilesChecked; }
            set
            {
                SettingsClient.Set<bool>("Indexing", "IgnoreRemovedFiles", value);
                SetProperty<bool>(ref this.checkBoxIgnoreRemovedFilesChecked, value);

                if (!value)
                {
                    // Fire and forget
                    trackRepository.ClearRemovedTrack(); 
                }
            }
        }

        public bool CheckBoxRereadInfoChecked
        {
            get { return this.checkBoxRereadInfoChecked; }
            set
            {
                SetProperty<bool>(ref this.checkBoxRereadInfoChecked, value);
            }
        }

        public bool CheckBoxDownloadMissingAlbumCoversChecked
        {
            get { return this.checkBoxDownloadMissingAlbumCoversChecked; }
            set
            {
                SettingsClient.Set<bool>("Covers", "DownloadMissingAlbumCovers", value);
                SetProperty<bool>(ref this.checkBoxDownloadMissingAlbumCoversChecked, value);

                if (value)
                {
                    this.indexingService.RetrieveInfoAsync(true, false);
                }
            }
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxRefreshCollectionAutomaticallyChecked = SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically");
                this.checkBoxIgnoreRemovedFilesChecked = SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles");
                this.checkBoxDownloadMissingAlbumCoversChecked = SettingsClient.Get<bool>("Covers", "DownloadMissingAlbumCovers");
            });
        }
    }
}
