﻿using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("ArtistImages")]
    public class ArtistImage
    {
        [Column("artist_id"), PrimaryKey()]
        public long ArtistId { get; set; }

        [Column("location"), NotNull()]
        public string Location { get; set; }

        [Column("source")]
        public string Source { get; set; }

        [Column("date_added")]
        public long DateAdded { get; set; }

    }
}
