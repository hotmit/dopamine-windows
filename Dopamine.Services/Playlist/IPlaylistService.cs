﻿using Dopamine.Data.Entities;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public interface IPlaylistService
    {
        string PlaylistFolder { get; }

        string DialogFileFilter { get; }

        event TracksAddedHandler TracksAdded;
        event TracksDeletedHandler TracksDeleted;
        event EventHandler PlaylistFolderChanged;

        Task<CreateNewPlaylistResult> CreateNewPlaylistAsync(EditablePlaylistViewModel editablePlaylist);

        Task<AddTracksToPlaylistResult> AddTracksToStaticPlaylistAsync(IList<TrackViewModel> tracks, string playlistName);

        Task<AddTracksToPlaylistResult> AddArtistsToStaticPlaylistAsync(IList<ArtistViewModel> artists, string playlistName);

        Task<AddTracksToPlaylistResult> AddGenresToStaticPlaylistAsync(IList<GenreViewModel> genres, string playlistName);

        Task<AddTracksToPlaylistResult> AddAlbumsToStaticPlaylistAsync(IList<AlbumViewModel> albumViewModels, string playlistName);

        Task<IList<PlaylistViewModel>> GetStaticPlaylistsAsync();

        Task<IList<PlaylistViewModel>> GetAllPlaylistsAsync();

        Task<IList<TrackViewModel>> GetTracksAsync(PlaylistViewModel playlist);

        Task<DeleteTracksFromPlaylistResult> DeleteTracksFromStaticPlaylistAsync(IList<string> playlistEntries, PlaylistViewModel playlist);

        Task<string> GetUniquePlaylistNameAsync(string proposedPlaylistName);

        Task<EditPlaylistResult> EditPlaylistAsync(EditablePlaylistViewModel editablePlaylistViewModel);

        Task<DeletePlaylistsResult> DeletePlaylistAsync(PlaylistViewModel playlist);

        Task SetStaticPlaylistOrderAsync(PlaylistViewModel playlist, IList<TrackViewModel> tracks);

        Task<ImportPlaylistResult> ImportPlaylistsAsync(IList<string> fileNames);

        Task<EditablePlaylistViewModel> GetEditablePlaylistAsync(PlaylistViewModel playlistViewModel);
    }
}
