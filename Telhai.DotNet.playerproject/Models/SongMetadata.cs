namespace Telhai.DotNet.PlayerProject
{
    public class SongMetadata
    {
        public string? TrackName { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? ApiArtworkUrl { get; set; }

        public List<string> Images { get; set; } = new List<string>();

    }
}
