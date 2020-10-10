﻿using Dopamine.Core.Alex;
using Dopamine.Core.Extensions;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Data.UnitOfWorks
{
    public class SQLiteUpdateCollectionUnitOfWork: IUpdateCollectionUnitOfWork
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private SQLiteConnection conn;
        private SQLiteTrackVRepository sQLiteTrackVRepository;
        private SQLiteInfoRepository sQLiteImageRepository;
        private bool bSharedConnection;
        public SQLiteUpdateCollectionUnitOfWork(SQLiteConnection conn, bool bSharedConnection)
        {
            this.bSharedConnection = bSharedConnection;
            this.conn = conn;
            if (!bSharedConnection)
                this.conn.BeginTransaction();
            sQLiteTrackVRepository = new SQLiteTrackVRepository(null);
            sQLiteImageRepository = new SQLiteInfoRepository(null);
            sQLiteTrackVRepository.SetSQLiteConnection(conn);
            sQLiteImageRepository.SetSQLiteConnection(conn);

        }

        public void Dispose()
        {
            if (!bSharedConnection)
            {
                conn.Commit();
                conn.Dispose();
            }
        }

        public AddMediaFileResult AddMediaFile(MediaFileData mediaFileData, long folderId)
        {
            AddMediaFileResult result = new AddMediaFileResult() { Success=false };
            try
            {
                int added = conn.Insert(new Track2()
                {
                    Name = mediaFileData.Name,
                    Path = mediaFileData.Path,
                    FolderId = folderId,
                    Filesize = mediaFileData.Filesize,
                    Bitrate = mediaFileData.Bitrate,
                    Samplerate = mediaFileData.Samplerate,
                    Duration = mediaFileData.Duration,
                    Year = mediaFileData.Year,
                    Language = mediaFileData.Language,
                    DateAdded = mediaFileData.DateAdded,
                    Rating = mediaFileData.Rating,//Should you take it from the file?
                    Love = mediaFileData.Love,
                    DateFileCreated = mediaFileData.DateFileCreated,
                    DateFileModified = mediaFileData.DateFileModified,
                    DateFileDeleted = mediaFileData.DateFileDeleted,
                    DateIgnored = mediaFileData.DateIgnored
                });
                if (added == 0)
                    return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format("AddMediaFile / Track2 Path: {0} Exce: {1}", mediaFileData.Path, ex.Message));
                return result;
            }
            result.TrackId = GetLastInsertRowID();
            //Add the (Album) artists in an artistCollection list
            List<long> artistCollection = new List<long>();
            if (!ListExtensions.IsNullOrEmpty<string>(mediaFileData.AlbumArtists))
            {
                mediaFileData.AlbumArtists = mediaFileData.AlbumArtists.Distinct().ToList();
                foreach (string artist in mediaFileData.AlbumArtists)
                {
                    artistCollection.Add(GetArtistID(artist));
                }
            }
            //Add the artists
            if (!ListExtensions.IsNullOrEmpty<string>(mediaFileData.Artists))
            {
                bool bUseArtistForAlbumArtistCollection = artistCollection.Count == 0;
                mediaFileData.Artists = mediaFileData.Artists.Distinct().ToList();
                foreach (string artist in mediaFileData.Artists)
                {
                    long curID = GetArtistID(artist);
                    if (bUseArtistForAlbumArtistCollection)
                        artistCollection.Add(curID);
                    conn.Insert(new TrackArtist()
                    {
                        TrackId = (long) result.TrackId,
                        ArtistId = curID,
                        ArtistRoleId = 1
                    });
                }
            }

            if (!string.IsNullOrEmpty(mediaFileData.Album))
            {
                long artistCollectionID = GetArtistCollectionID(artistCollection);
                result.AlbumId = GetAlbumID(mediaFileData.Album, artistCollectionID);
                try
                {
                    conn.Insert(new TrackAlbum()
                    {
                        TrackId = (long) result.TrackId,
                        AlbumId = (long) result.AlbumId,
                        TrackNumber = mediaFileData.TrackNumber,
                        DiscNumber = mediaFileData.DiscNumber,
                        TrackCount = mediaFileData.TrackCount,
                        DiscCount = mediaFileData.DiscCount
                    });
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine(String.Format("SQLiteException (Genres) {0}", ex.Message));
                }
            }


            if (!ListExtensions.IsNullOrEmpty<string>(mediaFileData.Genres))
            {
                mediaFileData.Genres = mediaFileData.Genres.Distinct().ToList();
                foreach (string genre in mediaFileData.Genres)
                {
                    long curID = GetGenreID(genre);
                    try
                    {
                        conn.Insert(new TrackGenre()
                        {
                            TrackId = (long) result.TrackId,
                            GenreId = curID
                        });
                    }
                    catch (SQLite.SQLiteException ex)
                    {
                        Debug.WriteLine(String.Format("SQLiteException (Genres) {0}", ex.Message));
                    }

                }
            }

            if (mediaFileData.Lyrics != null && mediaFileData.Lyrics.Text != null && mediaFileData.Lyrics.Text.Length > 0)
            {
                try
                {
                    conn.Insert(new TrackLyrics()
                    {
                        TrackId = (long)result.TrackId,
                        Lyrics = mediaFileData.Lyrics.Text,
                        Source = mediaFileData.Lyrics.Source,
                        DateAdded = DateTime.Now.Ticks,
                        Language = mediaFileData.Lyrics.Language
                    }); ;
                }
                catch (SQLiteException ex)
                {
                    Debug.WriteLine(String.Format("SQLiteException (TrackLyrics) {0}", ex.Message));
                }
            }
            result.Success = true;
            return result;
        }

        public UpdateMediaFileResult UpdateMediaFile(TrackV trackV, MediaFileData mediaFileData)
        {
            UpdateMediaFileResult updateMediaFileResult = new UpdateMediaFileResult() { Success = false };
            long track_id = trackV.Id;
            long folder_id = trackV.FolderID;
            int success = conn.Update(new Track2()
            {
                Id = track_id,
                Name = mediaFileData.Name,
                Path = mediaFileData.Path,
                FolderId = folder_id,
                Filesize = mediaFileData.Filesize,
                Bitrate = mediaFileData.Bitrate,
                Samplerate = mediaFileData.Samplerate,
                Duration = mediaFileData.Duration,
                Year = mediaFileData.Year,
                Language = mediaFileData.Language,
                DateAdded = mediaFileData.DateAdded,
                Rating = mediaFileData.Rating,
                Love = mediaFileData.Love,
                DateFileCreated = mediaFileData.DateFileCreated,
                DateFileModified = mediaFileData.DateFileModified,
                DateFileDeleted = mediaFileData.DateFileDeleted,
                DateIgnored = mediaFileData.DateIgnored
            });
            if (success == 0)
                return updateMediaFileResult;
            //Add the (Album) artists in an artistCollection list
            List<long> artistCollection = new List<long>();
            if (!ListExtensions.IsNullOrEmpty<string>(mediaFileData.AlbumArtists))
            {
                mediaFileData.AlbumArtists = mediaFileData.AlbumArtists.Distinct().ToList();
                foreach (string artist in mediaFileData.AlbumArtists)
                {
                    artistCollection.Add(GetArtistID(artist));
                }
            }
            //Add the artists
            conn.Execute(String.Format("DELETE FROM TrackArtists WHERE track_id={0}", track_id));
            if (!ListExtensions.IsNullOrEmpty<string>(mediaFileData.Artists))
            {
                mediaFileData.Artists = mediaFileData.Artists.Distinct().ToList();
                bool bUseArtistForAlbumArtistCollection = artistCollection.Count == 0;

                foreach (string artist in mediaFileData.Artists)
                {
                    long curID = GetArtistID(artist);
                    if (bUseArtistForAlbumArtistCollection)
                        artistCollection.Add(curID);
                    conn.Insert(new TrackArtist()
                    {
                        TrackId = track_id,
                        ArtistId = curID,
                        ArtistRoleId = 1
                    });
                }
            }

            conn.Execute(String.Format("DELETE FROM TrackAlbums WHERE track_id={0}", track_id));
            if (!string.IsNullOrEmpty(mediaFileData.Album))
            {
                long artistCollectionID = GetArtistCollectionID(artistCollection);
                updateMediaFileResult.AlbumId = GetAlbumID(mediaFileData.Album, artistCollectionID);
                try
                {
                    conn.Insert(new TrackAlbum()
                    {
                        TrackId = track_id,
                        AlbumId = (long) updateMediaFileResult.AlbumId,
                        TrackNumber = mediaFileData.TrackNumber,
                        DiscNumber = mediaFileData.DiscNumber,
                        TrackCount = mediaFileData.TrackCount,
                        DiscCount = mediaFileData.DiscCount
                    });
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine(String.Format("SQLiteException (Genres) {0}", ex.Message));
                }
            }


            conn.Execute(String.Format("DELETE FROM TrackGenres WHERE track_id={0}", track_id));
            if (!ListExtensions.IsNullOrEmpty<string>(mediaFileData.Genres))
            {
                mediaFileData.Genres = mediaFileData.Genres.Distinct().ToList();
                foreach (string genre in mediaFileData.Genres)
                {
                    long curID = GetGenreID(genre);
                    try
                    {
                        conn.Insert(new TrackGenre()
                        {
                            TrackId = track_id,
                            GenreId = curID
                        });
                    }
                    catch (SQLite.SQLiteException ex)
                    {
                        Debug.WriteLine(String.Format("SQLiteException (Genres) {0}", ex.Message));
                    }

                }
            }

            conn.Execute(String.Format("DELETE FROM TrackLyrics WHERE track_id={0}", track_id));
            if (mediaFileData.Lyrics != null)
            {
                try
                {
                    conn.Insert(new TrackLyrics()
                    {
                        TrackId = track_id,
                        Lyrics = mediaFileData.Lyrics.Text,
                        Source = mediaFileData.Lyrics.Source,
                        DateAdded = DateTime.Now.Ticks,
                        Language = mediaFileData.Lyrics.Language
                    }); ;
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine(String.Format("SQLiteException (TrackLyrics) {0}", ex.Message));
                }
            }
            updateMediaFileResult.Success = true;
            return updateMediaFileResult;
        }

        public TrackV GetTrackWithPath(string path)
        {
            return sQLiteTrackVRepository.GetTrackWithPath(path, new QueryOptions() {WhereDeleted = QueryOptionsBool.Ignore, WhereIgnored = QueryOptionsBool.Ignore, WhereVisibleFolders = QueryOptionsBool.Ignore, UseLimit=false });
        }


        public bool SetAlbumImage(AlbumImage image, bool replaceIfExists)
        {
            Debug.Assert(image.AlbumId > 0);
            Debug.Assert(image.Location.Length > 10);
            Logger.Debug("SetAlbumImage {0}", image.Location);

            try
            {
                //image.DateAdded = DateTime.Now.Ticks;
                if (replaceIfExists)
                    conn.InsertOrReplace(image);
                else
                    conn.Insert(image);
                return true;
                //return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                Logger.Error(ex, "SetAlbumImage");
            }
            return false;
        }

        public bool RemoveAlbumImage(long album_id)
        {
            int deletions = conn.Execute("DELETE FROM AlbumImages WHERE album_id=?", album_id);
            if (deletions == 0)
                return false;
            return true;
        }

        private long GetArtistID(String entry)
        {
            //=== Normalization. Clean up "the " from artists
            if (entry.ToLower().StartsWith("the "))
                entry = entry.Substring(4);
            //=== END Normalization
            long? id = conn.ExecuteScalar<long?>("SELECT id FROM Artists WHERE name=?", entry);
            if (id != null)
                return (long)id;
            try
            {
                conn.Insert(new Artist() { Name = entry });
                return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                Debug.WriteLine(String.Format("SQLiteException (GetArtistID) {0}", ex.Message));
                throw new Exception(String.Format("SQLiteException (GetArtistID) '{0}' ex:{1}", entry, ex.Message));
            }
        }

        private long GetArtistCollectionID(List<long> artistIDs)
        {
            string inString = string.Join(",", artistIDs);
            long? id = conn.ExecuteScalar<long?>(@"
SELECT DISTINCT artist_collection_id from ArtistCollectionsArtists 
INNER JOIN (
SELECT artist_collection_id as id, count(*) as c FROM ArtistCollectionsArtists
GROUP BY artist_collection_id) AGROUP ON ArtistCollectionsArtists.artist_collection_id = AGROUP.id
WHERE artist_id IN (" + inString + ") AND AGROUP.C=" + artistIDs.Count.ToString());

            if (id != null)
                return (long)id;
            conn.Insert(new ArtistCollection() { });
            long artist_collection_id = GetLastInsertRowID();
            foreach (long artistID in artistIDs)
            {
                conn.Insert(new ArtistCollectionsArtist() { ArtistCollectionId = artist_collection_id, ArtistId = artistID }); ;
            }
            return artist_collection_id;
        }




        private long GetAlbumID(String entry, long artist_collection_id)
        {
            long? id = conn.ExecuteScalar<long?>("SELECT id FROM Albums WHERE name=? AND artist_collection_ID=?", entry, artist_collection_id);
            if (id != null)
                return (long) id;
            try
            {
                conn.Insert(new Album() { Name = entry, ArtistCollectionId = artist_collection_id });
                return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                Debug.WriteLine(String.Format("SQLiteException (GetAlbumID) {0}", ex.Message));
                throw new Exception(String.Format("SQLiteException (GetAlbumID) '{0}' ex:{1}", entry, ex.Message));
            }

        }

        /*
        private long GetAlbumImageID(AlbumImage image)
        {
            long? id = conn.ExecuteScalar<long?>("SELECT id FROM AlbumImages WHERE album_id=? AND location=?", image.AlbumId, image.Location);
            //long? id = conn.ExecuteScalar<long?>("SELECT id FROM AlbumImages WHERE path=?", image.Path);
            if (id != null)
                return (long) id;
            try
            {
                //image.DateAdded = DateTime.Now.Ticks;
                conn.Insert(image);
                return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                string err = String.Format("SQLiteException (GetAlbumImageID) '{0}' ex:{1}", image.Location, ex.Message);
                Debug.WriteLine(err);
                throw new Exception(err);
            }
        }
        */
        /*
        private long GetArtistImageID(ArtistImage image)
        {
            long? id = conn.ExecuteScalar<long?>("SELECT id FROM ArtistImages WHERE artist_id=? AND location=?", image.ArtistId, image.Location);
            //long? id = conn.ExecuteScalar<long?>("SELECT id FROM AlbumImages WHERE path=?", image.Path);
            if (id != null)
                return (long)id;
            try
            {
                image.DateAdded = DateTime.Now.Ticks;
                conn.Insert(image);
                return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                string err = String.Format("SQLiteException (GetArtistImageID) '{0}' ex:{1}", image.Location, ex.Message);
                Debug.WriteLine(err);
                throw new Exception(err);
            }
        }
        */
        private long GetGenreID(String entry)
        {
            long? id = conn.ExecuteScalar<long?>("SELECT id FROM Genres WHERE name=?", entry);
            if (id != null)
                return (long) id;
            try
            {
                conn.Insert(new Genre() { Name = entry });
                return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                string err = String.Format("SQLiteException (GetGenreID) '{0}' ex:{1}", entry, ex.Message);
                Debug.WriteLine(err);
                throw new Exception(err);
            }
        }

        private long GetLastInsertRowID()
        {
            SQLiteCommand cmdLastRow = conn.CreateCommand(@"select last_insert_rowid()");
            return cmdLastRow.ExecuteScalar<long>();
        }


        public bool SetArtistImage(ArtistImage image, bool replaceIfExists)
        {
            Debug.Assert(image.ArtistId > 0);
            Debug.Assert(image.Location.Length > 10);
            Logger.Debug("SetArtistImage {0}", image.Location);

            try
            {
                //image.DateAdded = DateTime.Now.Ticks;
                if (replaceIfExists)
                    conn.InsertOrReplace(image);
                else
                    conn.Insert(image);
                return true;
                //return GetLastInsertRowID();
            }
            catch (SQLite.SQLiteException ex)
            {
                Logger.Error(ex, "SetArtistImage");
            }
            return false;
        }


        public bool RemoveArtistImage(long artist_id)
        {
            int deletions = conn.Execute("DELETE FROM ArtistImages WHERE artist_id=?", artist_id);
            if (deletions == 0)
                return false;
            return true;
        }

        public bool SetLyrics(TrackLyrics trackLyrics, bool replaceIfExists)
        {
            Debug.Assert(trackLyrics.TrackId > 0);
            Debug.Assert(trackLyrics.Lyrics.Length > 0);
            Logger.Debug("SetLyrics");

            try
            {
                //image.DateAdded = DateTime.Now.Ticks;
                if (replaceIfExists)
                    conn.InsertOrReplace(trackLyrics);
                else
                    conn.Insert(trackLyrics);
                return true;
                //return GetLastInsertRowID();
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Equals("UNIQUE constraint failed: TrackLyrics.track_id"))
                {
                    //=== Already Exists
                    Debug.Assert(replaceIfExists == false);
                }
                else
                    Logger.Error(ex, "SetLyrics");
            }
            return false;
        }


        public bool RemoveLyrics(long track_id)
        {
            int deletions = conn.Execute("DELETE FROM TrackLyrics WHERE track_id=?", track_id);
            if (deletions == 0)
                return false;
            return true;
        }


        public bool SetArtistBiography(ArtistBiography artistBiography)
        {
            Logger.Debug("SetArtistBiography");
            Debug.Assert(artistBiography != null);
            Debug.Assert(artistBiography.ArtistId > 0);
            Debug.Assert(artistBiography.Biography.Length > 0);
            try
            {
                int ret = conn.InsertOrReplace(artistBiography);
                return true;
            }
            catch (SQLiteException ex)
            {
                Logger.Error(ex, "SetArtistBiography");
            }
            return false;
        }
        public bool SetAlbumReview(AlbumReview albumReview)
        {
            Logger.Debug("SetAlbumReview");
            Debug.Assert(albumReview != null);
            Debug.Assert(albumReview.AlbumId > 0);
            Debug.Assert(albumReview.Review.Length > 0);
            try
            {
                int ret = conn.InsertOrReplace(albumReview);
                return true;
            }
            catch (SQLiteException ex)
            {
                Logger.Error(ex, "SetAlbumReview");
            }
            return false;
        }

        public bool SetAlbumImageFailed(AlbumV album)
        {
            return conn.InsertOrReplace(new AlbumImageFailed()
            {
                AlbumId = album.Id,
                DateAdded = DateTime.Now.Ticks
            }) > 0;
        }
        public bool SetArtistImageFailed(ArtistV artist)
        {
            return conn.InsertOrReplace(new ArtistImageFailed()
            {
                ArtistId = artist.Id,
                DateAdded = DateTime.Now.Ticks
            }) > 0;
        }

        public bool HasArtistImageFailed(ArtistV artist)
        {
            List<long> ids = conn.Query<long>("SELECT artist_id from ArtistImageFailed WHERE artist_id=?", artist.Id);
            return !ids.IsNullOrEmpty();
        }

        public bool HasAlbumImageFailed(AlbumV album)
        {
            List<long> ids = conn.Query<long>("SELECT album_id from AlbumImageFailed WHERE album_id=?", album.Id);
            return !ids.IsNullOrEmpty();
        }


        public bool ClearAlbumImageFailed(AlbumV album)
        {
            return conn.Execute("DELETE FROM AlbumImageFailed WHERE album_id=?", album.Id) > 0;
        }
        public bool ClearArtistImageFailed(ArtistV artist)
        {
            return conn.Execute("DELETE FROM ArtistImageFailed WHERE artist_id=?", artist.Id) > 0;
        }


    }


}
