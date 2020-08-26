﻿using Digimezzo.Foundation.Core.Logging;
using Dopamine.Data;
using Dopamine.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Services.Indexing
{
    internal class IndexerCache
    {
        private Dictionary<long, TrackV> cachedTracks;

        private ISQLiteConnectionFactory factory;

        public IndexerCache(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public bool HasCachedTrack(ref TrackV track)
        {
            bool hasCachedTrack = false;
            long similarTrackId = 0;

            TrackV tempTrack = track;

            try
            {
                similarTrackId = this.cachedTracks.Where((t) => t.Value.Equals(tempTrack)).Select((t) => t.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem checking if Track with path '{0}' exists in the cache. Exception: {1}", track.Path, ex.Message);
            }

            if (similarTrackId != 0)
            {
                hasCachedTrack = true;
                track.Id = similarTrackId;
            }

            return hasCachedTrack;
        }

        public void AddTrack(TrackV track)
        {
            this.cachedTracks.Add(track.Id, track);
        }

        public void Initialize()
        {
            // Comparing new and existing objects will happen in a Dictionary cache. This should improve performance.
            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                this.cachedTracks = conn.Table<TrackV>().ToDictionary(trk => trk.Id, trk => trk);
            }
        }
    }
}
