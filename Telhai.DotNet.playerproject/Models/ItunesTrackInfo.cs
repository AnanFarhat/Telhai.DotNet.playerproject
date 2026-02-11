using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Telhai.DotNet.PlayerProject.Models;

public class ItunesTrackInfo
{
    [JsonPropertyName("trackName")]
    public string? TrackName { get; set; }

    [JsonPropertyName("artistName")]
    public string? ArtistName { get; set; }

    [JsonPropertyName("collectionName")]
    public string? AlbumName { get; set; }

    [JsonPropertyName("artworkUrl100")]
    public string? ArtworkUrl { get; set; }
}

