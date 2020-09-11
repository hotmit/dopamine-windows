﻿using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("TrackAlbums")]
    public class TrackAlbum
    {
        [Column("track_id"), PrimaryKey(), NotNull()]
        public long TrackId { get; set; }

        [Column("album_id"), Indexed(), NotNull()]
        public long AlbumId { get; set; }

        [Column("track_number")]
        public long? TrackNumber { get; set; }

        [Column("disc_number")]
        public long? DiscNumber { get; set; }

        [Column("track_count")]
        public long? TrackCount { get; set; }

        [Column("disc_count")]
        public long? DiscCount { get; set; }


    }
}
