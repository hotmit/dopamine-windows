﻿using Dopamine.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.ViewModels.Common
{
    public class PlayAllControlViewModel : BindableBase
    {
        private IPlaybackService playbackService;
       
        public DelegateCommand PlayAllCommand { get; set; }
     
        public PlayAllControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.PlayAllCommand = new DelegateCommand(() => {
                this.playbackService.PlayAllTracksAsync(PlaylistMode.Play);
                });
        }
    }
}
