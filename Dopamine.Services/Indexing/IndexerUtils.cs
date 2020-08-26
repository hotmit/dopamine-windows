﻿using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using System;
using System.IO;

namespace Dopamine.Services.Indexing
{
    public static class IndexerUtils
    {
        public static bool IsTrackOutdated(TrackV track)
        {
            if (track.FileSize == null || track.FileSize != FileUtils.SizeInBytes(track.Path) || track.DateFileModified < FileUtils.DateModifiedTicks(track.Path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetExternalArtworkPath(string path)
        {
            string directory = Path.GetDirectoryName(path);

            foreach (string externalCoverArtPattern in Defaults.ExternalCoverArtPatterns)
            {
                var filename = (Path.Combine(directory, externalCoverArtPattern.Replace("%filename%", Path.GetFileNameWithoutExtension(path))));

                if (System.IO.File.Exists(filename))
                {
                    return filename;
                }
            }

            return string.Empty;
        }

        public static byte[] GetEmbeddedArtwork(FileMetadata fileMetadata)
        {

            byte[] artworkData = null;

            try
            {
                artworkData = fileMetadata.ArtworkData.Value;
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem while getting artwork data for Track with path='{0}'. Exception: {1}", fileMetadata.Path, ex.Message);
            }

            return artworkData;
        }

        public static byte[] GetExternalArtwork(string path, int width, int height)
        {
            byte[] artworkData = null;

            try
            {
                string externalArtworkPath = GetExternalArtworkPath(path);

                if (!string.IsNullOrEmpty(externalArtworkPath))
                {
                    artworkData = ImageUtils.Image2ByteArray(externalArtworkPath, width, height);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem while getting external artwork for Track with path='{0}'. Exception: {1}", path, ex.Message);
            }

            return artworkData;
        }

        public static byte[] GetArtwork(long albumId, FileMetadata fileMetadata)
        {
            byte[] artworkData = null;

            try
            {
                // Don't get artwork if the album is unknown
                //ALEX=== if (!string.IsNullOrEmpty(albumKey))
                {
                    // Get embedded artwork
                    artworkData = GetEmbeddedArtwork(fileMetadata);

                    if (artworkData == null || artworkData.Length == 0)
                    {
                        // If getting embedded artwork failed, try to get external artwork.
                        artworkData = GetExternalArtwork(fileMetadata.Path, 0, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not get artwork for Album with albumId='{albumId}'. Exception: {ex.Message}");
            }

            return artworkData;
        }

        public static long CalculateSaveItemCount(long numberItemsToAdd)
        {
            if (numberItemsToAdd < 50000)
            {
                return 5000;
                // Every 5000 items
            }
            else
            {
                return Convert.ToInt64((10 / 100) * numberItemsToAdd);
                // Save every 10 %
            }
        }

        public static int CalculatePercent(long currentValue, long totalValue)
        {
            int percent = 0;

            if (totalValue > 0)
            {
                percent = Convert.ToInt32((currentValue / (double)totalValue) * 100);
            }

            return percent;
        }
    }

}
