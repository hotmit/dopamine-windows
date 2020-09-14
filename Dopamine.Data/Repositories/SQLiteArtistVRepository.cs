﻿using Dopamine.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Digimezzo.Foundation.Core.Logging;

namespace Dopamine.Data.Repositories
{
    public class SQLiteArtistVRepository: IArtistVRepository
    {
        private readonly ISQLiteConnectionFactory factory;

        public SQLiteArtistVRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public List<ArtistV> GetArtists()
        {
            return GetArtistsInternal();
        }

        public List<ArtistV> GetArtistToIndexByProvider(string provider, bool includeFailed)
        {
            QueryOptions qo = new QueryOptions();
            qo.extraWhereClause.Add("ArtistImages.artist_id is null");
            if (!includeFailed)
            {
                qo.extraJoinClause.Add("LEFT JOIN ArtistImageFailed ON ArtistImageFailed.artist_id=Artists.id");
                qo.extraJoinParams.Add(provider);
                qo.extraWhereClause.Add("ArtistImageFailed.artist_id is null");
            }
            return GetArtistsInternal(qo);
        }

        private List<ArtistV> GetArtistsInternal(QueryOptions queryOptions = null)
        {
            try
            {
                using (var conn = factory.GetConnection())
                {
                    try
                    {
                        return RepositoryCommon.Query<ArtistV>(conn, GetSQLTemplate(), queryOptions);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Query Failed. Exception: {0}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
            }

            
            return null;
        }
        private string GetSQLTemplate()
        {
            return @"
SELECT
Artists.ID as Id,
Artists.name as Name,
COUNT(t.id) as TrackCount,
COUNT(DISTINCT Genres.id ) as GenreCount,
GROUP_CONCAT(DISTINCT Genres.name ) as Genres,
COUNT(DISTINCT Albums.id ) as AlbumCount,
GROUP_CONCAT(DISTINCT Albums.name ) as Albums,
MIN(t.year) as MinYear,
MAX(t.year) as MaxYear,
COALESCE(ArtistImages.location, AlbumImages.location) as Thumbnail,
MIN(t.date_added) as DateAdded,
MIN(t.date_file_created) as DateFileCreated
from Tracks t
LEFT JOIN TrackArtists  ON TrackArtists.track_id = t.id
LEFT JOIN Artists ON Artists.id = TrackArtists.artist_id
LEFT JOIN ArtistImages ON Artists.id = ArtistImages.artist_id
LEFT JOIN TrackAlbums ON TrackAlbums.track_id = t.id
LEFT JOIN Albums ON Albums.id = TrackAlbums.album_id
LEFT JOIN AlbumImages ON Albums.id = AlbumImages.album_id
LEFT JOIN TrackGenres ON TrackGenres.track_id = t.id
LEFT JOIN Genres ON TrackGenres.genre_id = Genres.id
#JOIN#
INNER JOIN Folders ON Folders.id = t.folder_id
#WHERE#
GROUP BY Artists.id
ORDER BY Artists.name
#LIMIT#
";
        }    
    }



}
