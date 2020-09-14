﻿using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("AlbumImages")]
    public class AlbumImage
    {
        [Column("album_id"), PrimaryKey()]
        public long AlbumId { get; set; }

        [Column("location"), NotNull()]
        public string Location { get; set; }

        [Column("source")]
        public string Source { get; set; }

        [Column("date_added")]
        public long DateAdded { get; set; }

    }
}
